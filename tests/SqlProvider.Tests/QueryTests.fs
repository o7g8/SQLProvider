﻿#if INTERACTIVE
#r @"../../bin/FSharp.Data.SqlProvider.dll"
#r @"../../packages/NUnit/lib/nunit.framework.dll"
#else
module QueryTests
#endif

open System
open FSharp.Data.Sql
open System.Linq
open NUnit.Framework

[<Literal>]
let connectionString = @"Data Source=./db/northwindEF.db;Version=3;Read Only=false;FailIfMissing=True;"

// If you want to run these in Visual Studio Test Explorer, please install:
// Tools -> Extensions and Updates... -> Online -> NUnit Test Adapter for Visual Studio
// http://nunit.org/index.php?p=vsTestAdapter&r=2.6.4

type sql = SqlDataProvider<Common.DatabaseProviderTypes.SQLITE, connectionString, CaseSensitivityChange=Common.CaseSensitivityChange.ORIGINAL>
FSharp.Data.Sql.Common.QueryEvents.SqlQueryEvent |> Event.add (printfn "Executing SQL: %s")
   
[<Test>]
let ``simple select with contains query``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            select cust.CustomerId
            contains "ALFKI"
        }
    Assert.IsTrue(qry)    

[<Test>]
let ``simple select with contains query with where``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.City <> "")
            select cust.CustomerId
            contains "ALFKI"
        }
    Assert.IsTrue(qry)    

[<Test>]
let ``simple select with contains query when not exists``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            select cust.CustomerId
            contains "ALFKI2"
        }
    Assert.IsFalse(qry)    

[<Test >]
let ``simple select with count``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            select cust.CustomerId
            count
        }
    Assert.AreEqual(91, qry)   

[<Test; Ignore("Not Supported")>]
let ``simple select with last``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            sortBy cust.CustomerId
            select cust.CustomerId
            last
        }
    Assert.AreEqual("WOLZA", qry)   

[<Test; Ignore("Not Supported")>]
let ``simple select with last or default when not exists``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.CustomerId = "ZZZZ")
            select cust.CustomerId
            lastOrDefault
        }
    Assert.AreEqual(null, qry)   

[<Test>]
let ``simple exists when exists``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            exists (cust.CustomerId = "WOLZA")
        }
    Assert.AreEqual(true, qry)  

[<Test; Ignore("Not Supported")>]
let ``simple select with last or default when exists``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.CustomerId = "WOLZA")
            select cust.CustomerId
            lastOrDefault
        }
    Assert.AreEqual("WOLZA", qry)  

[<Test >]
let ``simple select with exactly one``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.CustomerId = "ALFKI")
            select cust.CustomerId
            exactlyOne
        }
    Assert.AreEqual("ALFKI", qry)  

[<Test>]
let ``simple select with exactly one when not exists``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.CustomerId = "ZZZZ")
            select cust.CustomerId
            exactlyOneOrDefault
        }
    Assert.AreEqual(null, qry)  

[<Test >]
let ``simple select with head``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.CustomerId = "ALFKI")
            select cust.CustomerId
            head
        }
    Assert.AreEqual("ALFKI", qry)  

[<Test>]
let ``simple select with head or Default``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.CustomerId = "ALFKI")
            select cust.CustomerId
            headOrDefault
        }
    Assert.AreEqual("ALFKI", qry)  

[<Test>]
let ``simple select with head or Default when not exists``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.CustomerId = "ZZZZ")
            select cust.CustomerId
            headOrDefault
        }
    Assert.AreEqual(null, qry)  

[<Test >]
let ``simple select query``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            select cust
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry

[<Test>]
let ``simplest select query let temp``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            let y = cust.City
            select cust.Address
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry

[<Test>]
let ``simple select query let temp nested``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            let y1 = cust.Address
            let y2 = cust.City
            select (y1, y2)
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry

[<Test>]
let ``simple select query let temp``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            let y = cust.City + "test"
            select y
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry
    //Assert.AreEqual("Aachentest", qry.[0])
    //Assert.AreEqual("Berlintest", qry.[0])


[<Test>]
let ``simple select query let where``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            let y = cust.City + "test"
            where (cust.Address <> "")
            select y
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry
    Assert.AreEqual("Berlintest", qry.[0])

[<Test; Ignore("Not supported")>]
let ``simple select query let temp used in where``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            let y = cust.City + "test"
            where (y <> "")
            select y
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry
    Assert.AreEqual("Berlintest", qry.[0])


[<Test >]
let ``simple select query with operations in select``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            select (cust.Country + " " + cust.Address + (1).ToString())
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry

[<Test >]
let ``simple select where query``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.CustomerId = "ALFKI")
            select cust
        } |> Seq.toArray

    CollectionAssert.IsNotEmpty qry
    Assert.AreEqual(1, qry.Length)
    Assert.AreEqual("Berlin", qry.[0].City)

[<Test >]
let ``simple nth query``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            select cust
            nth 4
        }
    Assert.AreEqual("London", qry.City)


[<Test >]
let ``simple select where not query``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (not(cust.CustomerId = "ALFKI"))
            select cust
        } |> Seq.toArray

    CollectionAssert.IsNotEmpty qry
    Assert.AreEqual(90, qry.Length)

[<Test >]
let ``simple select where in query``() =
    let dc = sql.GetDataContext()
    let arr = ["ALFKI"; "ANATR"; "AROUT"]
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (arr.Contains(cust.CustomerId))
            select cust.CustomerId
        } |> Seq.toArray
    let res = query

    CollectionAssert.IsNotEmpty qry
    Assert.AreEqual(3, qry.Length)
    Assert.IsTrue(qry.Contains("ANATR"))

    
    
[<Test >]
let ``simple select where not-in query``() =
    let dc = sql.GetDataContext()
    let arr = ["ALFKI"; "ANATR"; "AROUT"]
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (not(arr.Contains(cust.CustomerId)))
            select cust.CustomerId
        } |> Seq.toArray
    let res = query

    CollectionAssert.IsNotEmpty qry
    Assert.AreEqual(88, qry.Length)
    Assert.IsFalse(qry.Contains("ANATR"))

[<Test >]
let ``simple select where in queryable query``() =

    let dc = sql.GetDataContext()
    let query1 = 
        query {
            for cust in dc.Main.Customers do
            where (cust.City="London")
            select cust.CustomerId
        }

    let query2 = 
        query {
            for cust in dc.Main.Customers do
            where (query1.Contains(cust.CustomerId))
            select cust.CustomerId
        } |> Seq.toArray
    let res = query

    CollectionAssert.IsNotEmpty query2
    Assert.AreEqual(6, query2.Length)
    Assert.IsTrue(query2.Contains("EASTC"))


[<Test >]
let ``simple select where in query custom syntax``() =
    let dc = sql.GetDataContext()
    let arr = ["ALFKI"; "ANATR"; "AROUT"]
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.CustomerId |=| arr)
            select cust.CustomerId
        } |> Seq.toArray

    CollectionAssert.IsNotEmpty qry
    Assert.AreEqual(3, qry.Length)
    Assert.IsTrue(qry.Contains("ANATR"))

[<Test >]
let ``simple select where like query``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.CustomerId.Contains("a"))
            select cust.CustomerId
        } |> Seq.toArray

    CollectionAssert.IsNotEmpty qry

[<Test >]
let ``simple select where query with operations in where``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.CustomerId = "ALFKI" && (cust.City.StartsWith("B")))
            select cust
        } |> Seq.toArray

    CollectionAssert.IsNotEmpty qry
    Assert.AreEqual(1, qry.Length)
    Assert.AreEqual("Berlin", qry.[0].City)

[<Test>]
let ``simple select query with minBy``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for ord in dc.Main.OrderDetails do
            minBy (decimal ord.Discount)
        }   
    Assert.AreEqual(0m, qry)

[<Test>]
let ``simple select query with minBy DateTime``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for emp in dc.Main.Employees do
            minBy (emp.BirthDate)
        }   
    Assert.AreEqual(DateTime(1937, 09, 19), qry)

[<Test>]
let ``simple select query with maxBy``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for od in dc.Main.OrderDetails do
            sumBy od.UnitPrice
        }
    Assert.Greater(56501m, qry)
    Assert.Less(56499m, qry)

[<Test>]
let ``simple select query with averageBy``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for od in dc.Main.OrderDetails do
            averageBy od.UnitPrice
        }
    Assert.Greater(27m, qry)
    Assert.Less(26m, qry)

[<Test; Ignore("Not Supported")>]
let ``simplest select query with groupBy``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            groupBy cust.City
        } |> Seq.toArray

    Assert.IsNotEmpty(qry)

[<Test; Ignore("Not Supported")>]
let ``simple select query with groupBy``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            groupBy cust.City into c
            select (c.Key, c.Count())
        }
    let res = qry |> dict  
    Assert.IsNotEmpty(res)
    Assert.AreEqual(6, res.["London"])
    

[<Test; Ignore("Not Supported")>]
let ``simple select query with groupBy date``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for emp in dc.Main.Employees do
            groupBy emp.BirthDate into e
            select (e.Key, e.Count())
        }
    let res = qry |> dict  
    Assert.IsNotEmpty(res)

[<Test; Ignore("Not Supported")>]
let ``simple select query with groupBy2``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            groupBy (cust.City, cust.Country) into c
            select (c.Key, c.Count()+1)
        } |> dict  
    Assert.IsNotNull(qry)

[<Test; Ignore("Not Supported")>]
let ``simple select query with groupValBy``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            groupValBy cust.ContactTitle cust.City into g
            select (g.Key, g.Count())
        } |> dict  
    Assert.IsNotEmpty(qry)

[<Test; Ignore("Not Supported")>]
let ``complex select query with groupValBy``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            groupValBy (cust.ContactTitle, Int32.Parse(cust.PostalCode)) (cust.PostalCode, cust.City) into g
            let maxPlusOne = 1 + query {for i in g do sumBy (snd i) }
            select (snd(g.Key), maxPlusOne)
        } |> dict  
    Assert.IsNotEmpty(qry)

[<Test;>]
let ``simple if query``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            if cust.Country = "UK" then select cust.City
        } |> Seq.toArray
    CollectionAssert.IsNotEmpty qry
    CollectionAssert.Contains(qry, "London")
    CollectionAssert.DoesNotContain(qry, "Helsinki")

[<Test;>]
let ``simple select query with case``() = 
    // Works but wrong implementation. Doesn't transfer logics to SQL.
    // Expected: SELECT CASE [cust].[Country] WHEN "UK" THEN [cust].[City] ELSE "Outside UK" END as 'City' FROM main.Customers as [cust]
    // Actual: SELECT [cust].[Country] as 'Country',[cust].[City] as 'City' FROM main.Customers as [cust]
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            select (if cust.Country = "UK" then (cust.City)
                else ("Outside UK"))
        } |> Seq.toArray
    CollectionAssert.IsNotEmpty qry

[<Test >]
let ``simple select and sort query``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            sortBy cust.City
            select cust.City
        } |> Seq.toArray

    CollectionAssert.IsNotEmpty qry    
    CollectionAssert.AreEquivalent([|"Aachen"; "Albuquerque"; "Anchorage"|], qry.[0..2])

[<Test >]
let ``simple select and sort desc query``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            sortByDescending cust.City
            select cust.City
        } |> Seq.toArray

    CollectionAssert.IsNotEmpty qry    
    CollectionAssert.AreEquivalent([|"Århus"; "Warszawa"; "Walla Walla"|], qry.[0..2])

[<Test>]
let ``simple select and sort query with then by query``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            sortBy cust.Country
            thenBy cust.City
            select cust.City
        } |> Seq.toArray

    CollectionAssert.IsNotEmpty qry    
    CollectionAssert.AreEquivalent([|"Buenos Aires"; "Buenos Aires"; "Buenos Aires"; "Graz"|], qry.[0..3])

[<Test>]
let ``simple select and sort query with then by desc query``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            sortBy cust.Country
            thenByDescending cust.City
            select cust.City
        } |> Seq.toArray

    CollectionAssert.IsNotEmpty qry    
    CollectionAssert.AreEquivalent([|"Buenos Aires"; "Buenos Aires"; "Buenos Aires"; "Salzburg"|], qry.[0..3])

[<Test >]
let ``simple select query with join``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            join order in dc.Main.Orders on (cust.CustomerId = order.CustomerId)
            select (cust.CustomerId, order.OrderDate)
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry
    CollectionAssert.AreEquivalent(
        [|
            "VINET", new DateTime(1996,7,4)
            "TOMSP", new DateTime(1996,7,5)
            "HANAR", new DateTime(1996,7,8)
            "VICTE", new DateTime(1996,7,8)
        |], qry.[0..3])


[<Test; Ignore("Grouping over multiple tables is not supported yet")>]
let ``simple select query with join and then groupBy``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            join order in dc.Main.Orders on (cust.CustomerId = order.CustomerId)
            groupBy cust.City into c
            select (c.Key, c.Count())
        }
    let res = qry |> dict  
    Assert.IsNotEmpty(res)
    Assert.AreEqual(6, res.["London"])

[<Test; Ignore("Joining over grouping is not supported yet")>]
let ``simple select query with groupBy and then join``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            groupBy cust.City into c
            join order in dc.Main.Orders on (c.Key = order.ShipCity)
            select (c.Key, c.Count())
        }
    let res = qry |> dict  
    Assert.IsNotEmpty(res)
    Assert.AreEqual(6, res.["London"])


[<Test >]
let ``simple select query with join multi columns``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            join order in dc.Main.Orders on ((cust.CustomerId, cust.CustomerId) = (order.CustomerId, order.CustomerId))
            select (cust.CustomerId, order.OrderDate)
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry
    CollectionAssert.AreEquivalent(
        [|
            "VINET", new DateTime(1996,7,4)
            "TOMSP", new DateTime(1996,7,5)
            "HANAR", new DateTime(1996,7,8)
            "VICTE", new DateTime(1996,7,8)
        |], qry.[0..3])


[<Test >]
let ``simple select query with join using relationships``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            for order in cust.``main.Orders by CustomerID`` do
            select (cust.CustomerId, order.OrderDate)
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry
    CollectionAssert.AreEquivalent(
        [|
            "VINET", new DateTime(1996,7,4)
            "TOMSP", new DateTime(1996,7,5)
            "HANAR", new DateTime(1996,7,8)
            "VICTE", new DateTime(1996,7,8)
        |], qry.[0..3])

[<Test; Ignore("Not Supported")>]
let ``simple select query with group join``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            groupJoin ord in dc.Main.Orders on (cust.CustomerId = ord.CustomerId) into g
            for order in g do
            join orderDetail in dc.Main.OrderDetails on (order.OrderId = orderDetail.OrderId)
            select (cust.CustomerId, orderDetail.ProductId, orderDetail.Quantity)
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry
    CollectionAssert.AreEquivalent(
        [|
            "VINET", 11L, 12
            "VINET", 42L, 10
            "VINET", 72L, 5
            "TOMSP", 14L, 9
            "TOMSP", 51L, 40
            "HANAR", 41L, 10
            "HANAR", 51L, 35
            "HANAR", 65L, 15
        |], qry.[0..7])

[<Test >]
let ``simple select query with multiple joins on relationships``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            for order in cust.``main.Orders by CustomerID`` do
            for orderDetail in order.``main.OrderDetails by OrderID`` do
            select (cust.CustomerId, orderDetail.ProductId, orderDetail.Quantity)
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry
    CollectionAssert.AreEquivalent(
        [|
            "VINET", 11L, 12s
            "VINET", 42L, 10s
            "VINET", 72L, 5s
            "TOMSP", 14L, 9s
            "TOMSP", 51L, 40s
            "HANAR", 41L, 10s
            "HANAR", 51L, 35s
            "HANAR", 65L, 15s
        |], qry.[0..7])

[<Test; Ignore("Not Supported")>]
let ``simple select query with left outer join``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            leftOuterJoin order in dc.Main.Orders on (cust.CustomerId = order.CustomerId) into result
            for order in result.DefaultIfEmpty() do
            select (cust.CustomerId, order.OrderDate)
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry
    CollectionAssert.AreEquivalent(
        [|
            "VINET", new DateTime(1996,7,4)
            "TOMSP", new DateTime(1996,7,5)
            "HANAR", new DateTime(1996,7,8)
            "VICTE", new DateTime(1996,7,8)
        |], qry.[0..3])

[<Test>]
let ``simple sumBy``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for od in dc.Main.OrderDetails do
            sumBy od.UnitPrice
        }
    Assert.That(qry, Is.EqualTo(56500.91M).Within(0.001M))

[<Test>]
let ``simple averageBy``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for od in dc.Main.OrderDetails do
            averageBy od.UnitPrice
        }
    Assert.That(qry, Is.EqualTo(26.2185m).Within(0.001M))

[<Test>]
let ``simple averageByNullable``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for od in dc.Main.OrderDetails do
            averageByNullable (System.Nullable(od.UnitPrice))
        }
    Assert.That(qry, Is.EqualTo(26.2185m).Within(0.001M))

[<Test >]
let ``simple select with distinct``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            select cust.City
            distinct
        } |> Seq.toArray

    CollectionAssert.IsNotEmpty qry   
    Assert.AreEqual(69, qry.Length) 
    CollectionAssert.AreEquivalent([|"Aachen"; "Albuquerque"; "Anchorage"|], qry.[0..2])

[<Test>]
let ``simple select with skip``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            select cust.City
            skip 5
        } 
        |> Seq.toArray

    CollectionAssert.IsNotEmpty qry   
    Assert.AreEqual(86, qry.Length) 
    CollectionAssert.AreEquivalent([|"Bergamo"; "Berlin"; "Bern"|], qry.[0..2])

[<Test >]
let ``simple select with take``() =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            select cust.City
            take 5
        } 
        |> Seq.toArray

    CollectionAssert.IsNotEmpty qry   
    Assert.AreEqual(5, qry.Length) 
    CollectionAssert.AreEquivalent([|"Aachen"; "Albuquerque"; "Anchorage"; "Barcelona"; "Barquisimeto"|], qry)  

[<Test>]
let ``simple select query with all``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for ord in dc.Main.OrderDetails do
            all (ord.UnitPrice > 0m)
        }   
    Assert.IsTrue(qry)

[<Test>]
let ``simple select query with all false``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for ord in dc.Main.OrderDetails do
            all (ord.UnitPrice > 10m)
        }   
    Assert.IsFalse(qry)

[<Test>]
let ``simple select query with find``() = 
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for ord in dc.Main.OrderDetails do
            find (ord.UnitPrice > 10m)
        }   
    Assert.AreEqual(14m, qry.UnitPrice)

type Simple = {First : string}

type Dummy<'t> = D of 't

[<Test >]
let ``simple select into a generic type`` () =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for emp in dc.Main.Customers do
            select (D {First=emp.ContactName})
        } |> Seq.toList

    CollectionAssert.IsNotEmpty qry

[<Test >]
let ``simple select into a generic type with pipe`` () =
    let dc = sql.GetDataContext()
    let qry = 
        query {
            for emp in dc.Main.Customers do
            select ({First=emp.ContactName} |> D)
        } |> Seq.toList

    CollectionAssert.IsNotEmpty qry

[<Test >]
let ``simple select with bool outside query``() = 
    let dc = sql.GetDataContext()
    let rnd = System.Random()
    // Direct booleans outside LINQ:
    let myCond1 = true
    let myCond2 = false
    let myCond3 = rnd.NextDouble() > 0.5
    let myCond4 = 4

    let qry = 
        query {
            for cust in dc.Main.Customers do
            // Simple booleans outside queries are supported:
            where (((myCond1 && myCond1=true) && cust.City="Helsinki" || myCond1) || cust.City="London")
            // Boolean in select fetches just either country or address, not both:
            select (if not(myCond3) then cust.Country else cust.Address)
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry


[<Test >]
let ``simple select with bool outside query2``() = 
    let dc = sql.GetDataContext()
    let rnd = System.Random()
    // Direct booleans outside LINQ:
    let myCond1 = true
    let myCond2 = false
    let myCond3 = rnd.NextDouble() > 0.5
    let myCond4 = 4

    let qry = 
        query {
            for cust in dc.Main.Customers do
            // Simple booleans outside queries are supported:
            where (myCond4 > 3 || (myCond2 && cust.Address="test" && not(myCond2)))
            // Boolean in select fetches just either country or address, not both:
            select (if not(myCond4=8) then cust.Country else cust.Address)
        } |> Seq.toArray
    
    CollectionAssert.IsNotEmpty qry

[<Test >]
let ``simple select query async``() = 
    let dc = sql.GetDataContext()
    let task = 
        async {
            let! asyncquery =
                query {
                    for cust in dc.Main.Customers do
                    select cust
                } |> Seq.executeQueryAsync 
            return asyncquery |> Seq.toList
        } |> Async.StartAsTask
    task.Wait()
    CollectionAssert.IsNotEmpty task.Result


type sqlOption = SqlDataProvider<Common.DatabaseProviderTypes.SQLITE, connectionString, CaseSensitivityChange=Common.CaseSensitivityChange.ORIGINAL, UseOptionTypes=true>
[<Test>]
let ``simple select with contains query with where boolean option type``() =
    let dc = sqlOption.GetDataContext()
    let qry = 
        query {
            for cust in dc.Main.Customers do
            where (cust.City.IsSome)
            select cust.CustomerId
            contains "ALFKI"
        }
    Assert.IsTrue(qry)    

[<Test>]
let ``simple union query test``() = 
    let dc = sql.GetDataContext()
    let query1 = 
        query {
            for cus in dc.Main.Customers do
            select (cus.City)
        }
    let query2 = 
        query {
            for emp in dc.Main.Employees do
            select (emp.City)
        } 

    // Union: query1 contains 69 distinct values, query2 distinct 5 and res1 is 71 distinct values
    let res1 = query1.Union(query2) |> Seq.toArray
    Assert.IsNotEmpty(res1)
    

[<Test>]
let ``simple union all query test``() = 
    let dc = sql.GetDataContext()
    let query1 = 
        query {
            for cus in dc.Main.Customers do
            select (cus.City)
        }
    let query2 = 
        query {
            for emp in dc.Main.Employees do
            select (emp.City)
        } 

    // Union all:
    // query1 contains 91 values and query2 contains 8 so res2 contains 99 values.
    let res2 = query1.Concat(query2) |> Seq.toArray
    Assert.IsNotEmpty(res2)
    
[<Test; Ignore("Not Supported")>]
let ``verify groupBy results``() = 
    let dc = sql.GetDataContext()
    let enumtest =
        query {
            for cust in dc.Main.Customers do
            select (cust)
        } |> Seq.toList
    let inlogics = 
        query {
            for cust in enumtest do
            groupBy cust.City into c
            select (c.Key, c.Count())
        } |> Seq.toArray |> Array.sortBy (fun (k,v) -> k )

    let groupqry = 
        query {
            for cust in dc.Main.Customers do
            groupBy cust.City into c
            select (c.Key, c.Count())
        } |> Seq.toArray |> Array.sortBy (fun (k,v) -> k )
    let res = groupqry |> dict  

    CollectionAssert.AreEqual(inlogics,groupqry)
