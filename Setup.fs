module Setup

open System

open Microsoft.Extensions.Logging
open LinqToDB
open LinqToDB.Mapping
open LinqToDB.Expressions
open LinqToDB.Data
open LinqToDB.DataProvider.PostgreSQL
open LinqToDB.FSharp
open Testcontainers.PostgreSql
open Npgsql

type ReferenceTypeOptionInfoProvider<'T when 'T :> obj and 'T: null and 'T : not struct>() =
    interface IGenericInfoProvider with
        member _.SetInfo(mappingSchema: MappingSchema) : unit =
            mappingSchema.SetConvertExpression<'T option, 'T>(Option.toObj) |> ignore

            mappingSchema.SetConvertExpression<'T, 'T option>(Option.ofObj) |> ignore

            mappingSchema.SetConverter<'T option, DataParameter>(fun d -> DataParameter(Value = Option.toObj d))
            |> ignore

type ValueTypeOptionInfoProvider<'T when 'T :> ValueType and 'T: struct and 'T: (new: unit -> 'T)>() =
    interface IGenericInfoProvider with
        member _.SetInfo(mappingSchema: MappingSchema) : unit =
            mappingSchema.SetConvertExpression<'T option, Nullable<'T>>(Option.toNullable)
            |> ignore

            mappingSchema.SetConvertExpression<Nullable<'T>, 'T option>(Option.ofNullable)
            |> ignore

            mappingSchema.SetConverter<'T option, DataParameter>(fun d -> DataParameter(Value = Option.toNullable d))
            |> ignore

type OptionInfoProvider<'T>() =
    let t = typeof<'T>

    interface IGenericInfoProvider with
        member _.SetInfo(mappingSchema: MappingSchema) : unit =
            let provider =
                if t.IsValueType then
                    let providerType = typedefof<ValueTypeOptionInfoProvider<_>>.MakeGenericType(t)
                    Activator.CreateInstance(providerType) :?> IGenericInfoProvider
                else
                    let providerType = typedefof<ReferenceTypeOptionInfoProvider<_>>.MakeGenericType(t)
                    Activator.CreateInstance(providerType) :?> IGenericInfoProvider

            provider.SetInfo(mappingSchema)


let createLoggerFactory () =
    LoggerFactory.Create(fun builder -> builder.AddConsole() |> ignore)

let createDataSource connectionString loggerFactory =
    let builder = new NpgsqlDataSourceBuilder(connectionString)

    builder.EnableParameterLogging() |> ignore

    builder.UseLoggerFactory(loggerFactory)
    |> ignore

    builder.Build()

let createDataOptions connectionString (dataSource: NpgsqlDataSource) =
    let dataProvider =
        PostgreSQLTools.GetDataProvider(connectionString = connectionString)

    let mappingSchema = MappingSchema()
    mappingSchema.SetGenericConvertProvider(typedefof<OptionInfoProvider<_>>)

    (new DataOptions())
        .UseConnectionFactory(dataProvider, (fun _ -> dataSource.CreateConnection()))
        .UseMappingSchema(mappingSchema)
        .UseFSharp()
