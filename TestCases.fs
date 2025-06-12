module OptionMergeTest

open System

open NUnit.Framework
open LinqToDB
open LinqToDB.Mapping
open LinqToDB.Data
open Testcontainers.PostgreSql
open FsUnitTyped

[<Table("my_optional_int")>]
[<CLIMutable>]
type MyOptionalInt = {
    [<Column("id"); PrimaryKey; Identity>]
    Id: int

    [<Column("value", DataType=DataType.Int32)>]
    Value: int option
}

let pg = PostgreSqlBuilder().WithImage("postgres:15-alpine").Build()
let mutable dataSource = null
let mutable dataOptions = null

[<OneTimeSetUp>]
let Setup () = task {
    do! pg.StartAsync()
    let connectionString = pg.GetConnectionString()
    let loggerFactory = Setup.createLoggerFactory()
    dataSource <- Setup.createDataSource connectionString loggerFactory

    // Create our table
    dataSource
        .CreateCommand(
            """
            create table my_optional_int (
                id int not null generated always as identity primary key,
                value int null
            )
            """
        )
        .ExecuteNonQuery()
    |> ignore

    dataOptions <- Setup.createDataOptions connectionString dataSource
}

[<OneTimeTearDown>]
let Teardown () = task {
    do! pg.DisposeAsync()
}

[<Test>]
let ``Can insert and select`` () =
    use conn = new DataConnection(dataOptions)
    let table = conn.GetTable<MyOptionalInt>()
    table.Truncate() |> ignore<int>

    let firstId = conn.InsertWithInt32Identity({ Id = 0; Value = Some 1})
    let secondId = conn.InsertWithInt32Identity({ Id = 0; Value = None })

    query {
        for entity in table do
            where (entity.Id = firstId)
            select entity.Value
            exactlyOne
    }
    |> shouldEqual (Some 1)
    
    query {
        for entity in table do
            where (entity.Id = secondId)
            select entity.Value
            exactlyOne
    }
    |> shouldEqual None
        
[<Test>]
let ``Can merge some`` () =
    use conn = new DataConnection(dataOptions)
    let table = conn.GetTable<MyOptionalInt>()
    table.Truncate() |> ignore<int>

    let entities: MyOptionalInt list = [
        { Id = 0; Value = Some 1}
        { Id = 0; Value = Some 2 }
    ]

    table.Merge().Using(entities).OnTargetKey().InsertWhenNotMatched().UpdateWhenMatched().Merge() |> shouldEqual 2

[<Test>]
let ``Can merge some and none`` () =
    use conn = new DataConnection(dataOptions)
    let table = conn.GetTable<MyOptionalInt>()
    table.Truncate() |> ignore<int>

    let entities: MyOptionalInt list = [
        { Id = 0; Value = Some 1}
        { Id = 0; Value = None }
    ]

    table.Merge().Using(entities).OnTargetKey().InsertWhenNotMatched().UpdateWhenMatched().Merge() |> shouldEqual 2


[<Test>]
let ``Can merge empty`` () =
    use conn = new DataConnection(dataOptions)
    let table = conn.GetTable<MyOptionalInt>()
    table.Truncate() |> ignore<int>

    let entities: MyOptionalInt list = []

    table.Merge().Using(entities).OnTargetKey().InsertWhenNotMatched().UpdateWhenMatched().Merge() |> shouldEqual 0


[<Test>]
let ``Can merge none`` () =
    use conn = new DataConnection(dataOptions)
    let table = conn.GetTable<MyOptionalInt>()
    table.Truncate() |> ignore<int>

    let entities: MyOptionalInt list = [
        { Id = 0; Value = None }
        { Id = 0; Value = None }
    ]

    table.Merge().Using(entities).OnTargetKey().InsertWhenNotMatched().UpdateWhenMatched().Merge() |> shouldEqual 2

