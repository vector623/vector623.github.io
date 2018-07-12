---
layout: post
title:  "Poor Man's Comparisons"
date:   2018-07-12 11:45:00 -0500
categories: humor
---

The `IComparer` and `IEqualityComparer` interfaces are highly useful to anyone writing functional code and dealing with
sets. As with most of my posts, I'm referencing the context of backend ETLs, which are processes that move data from one
place, to another.

If you are tasked with synchronizing a data destination against a source of that data, you will generally want to insert
new records, update existing and potentially delete old ones. Taking a functional approach, you will first want to
figure out which sales orders need to be created, updated and so on.

Using `IComparer` and `IEqualityComparer`, your ETL can look like this: 

```csharp
public class SalesOrder
{
    public int SalesOrderId;
    public DateTime CreatedAt;
    public DateTime LastModifiedAt;
    public string FirstName;
    public string LastName;
    public string Email;
    public string Phone;
    public string StreetAddress;
    public string City;
    public string State;
    public string Zipcode;
}

public void SyncSalesOrders() {
    var sourceSalesOrders = new List<SalesOrder>();
    var destionationSalesOrders = new List<SalesOrder>();

    var insertSalesOrders = sourceSalesOrders
        .Except(destionationSalesOrders, new SalesOrder.IdComparer())
        .ToList();
    var updateSalesOrders = sourceSalesOrders
        .Intersect(destionationSalesOrders, new SalesOrder.IdComparer())
        .Except(destionationSalesOrders, new SalesOrder.StatusComparer())
        .ToList();
    var deleteSalesOrders = destionationSalesOrders
        .Except(sourceSalesOrders, new SalesOrder.IdComparer())
        .ToList();

    ///...
}
```

Linq's `Except` and `Intersect` operators do all the heavy lifting for you after you define which orders are to be
included and excluded.  That definition takes places through fulfilling the `IComparer` and `IEqualityComparer`
interfaces:

```csharp
public class IdComparer : IComparer<SalesOrder>, IEqualityComparer<SalesOrder>
{
    public int Compare(SalesOrder x, SalesOrder y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        return x.SalesOrderId.CompareTo(y.SalesOrderId);
    }

    public bool Equals(SalesOrder x, SalesOrder y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.SalesOrderId == y.SalesOrderId;
    }

    public int GetHashCode(SalesOrder obj)
    {
        return obj.SalesOrderId;
    }
}

public class StatusComparer : IComparer<SalesOrder>, IEqualityComparer<SalesOrder>
{
    public bool Equals(SalesOrder x, SalesOrder y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.SalesOrderId == y.SalesOrderId && x.LastModifiedAt.Equals(y.LastModifiedAt);
    }

    public int GetHashCode(SalesOrder obj)
    {
        unchecked
        {
            return (obj.SalesOrderId * 397) ^ obj.LastModifiedAt.GetHashCode();
        }
    }

    public int Compare(SalesOrder x, SalesOrder y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        var salesOrderIdComparison = x.SalesOrderId.CompareTo(y.SalesOrderId);
        if (salesOrderIdComparison != 0) return salesOrderIdComparison;
        return x.LastModifiedAt.CompareTo(y.LastModifiedAt);
    }
}
```

In these interface implemntations, I am including only `SalesOrderId` when comparing identity.  For status, I include
both `SalesOrderId` and `LastModifiedDate`.  You can mix and match any fields in order to produce an implementation of
`IComparer` and `IEqualityComparer` that will evaluate only the fields you care about.

I prefer to let Jetbrains' excellent C# IDE, [Rider][0], generate that code for me. But you might be coding in Visual
Studio or even worse, Visual Studio Code, which don't provide automatic generators for these interfaces. If you need to
fulfill these interfaces yourself, you can leverage .NET's existing `String` implementation of the required methods:

```csharp
public class IdComparer : IComparer<SalesOrder>, IEqualityComparer<SalesOrder>
{
    public string ToString(SalesOrder obj)
    {
        return obj.SalesOrderId.ToString();
    }
    public int Compare(SalesOrder x, SalesOrder y)
    {
        return string.Compare(ToString(x), ToString(y));
    }
    public bool Equals(SalesOrder x, SalesOrder y)
    {
        return ToString(x).Equals(y);
    }
    public int GetHashCode(SalesOrder obj)
    {
        return ToString(obj).GetHashCode();
    }
}

public class StatusComparer : IComparer<SalesOrder>, IEqualityComparer<SalesOrder>
{
    public string ToString(SalesOrder obj)
    {
        return obj.SalesOrderId.ToString() +
               obj.LastModifiedAt.ToString();
    }
    public int Compare(SalesOrder x, SalesOrder y)
    {
        return string.Compare(ToString(x), ToString(y));
    }
    public bool Equals(SalesOrder x, SalesOrder y)
    {
        return ToString(x).Equals(y);
    }
    public int GetHashCode(SalesOrder obj)
    {
        return ToString(obj).GetHashCode();
    }
}
```

By adjusting which fields are included in the `ToString` return, you can augment the behavior of your comparators and
the nice thing about defaulting to the `String` implementations is that you don't really have to put a lot of thought
into implementing these methods.  They obviously won't be as fast if you implement this way, but they will be safe and
you don't have to do any real thinking beyond selecting the member variables you want to be compared.

If you want to consolidate the latter example, you can feed a `ToString` delegate into the `Compare`, `Equals` and 
`GetHashCode` methods.

[0]: https://www.jetbrains.com/rider/
