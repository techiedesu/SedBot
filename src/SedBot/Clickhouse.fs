module SedBot.Clickhouse

open System
open System.Text
open NUnit.Framework

type DataType =
    | UInt8
    | Int8
    | UInt16
    | Int16
    | UInt32
    | Int32
    | Float32
    | String
    | FixedString
    | Nullable of DataType

let private dataTypeToString (dt: DataType) =
    match dt with
    | Nullable (Nullable _) -> raise ^ ArgumentException("Nullable type can't contain nullable type")
    | UInt8 -> failwith "todo"
    | Int8 -> failwith "todo"
    | UInt16 -> failwith "todo"
    | Int16 -> failwith "todo"
    | UInt32 -> "UInt32"
    | Float32 -> failwith "todo"
    | String -> "String"
    | FixedString -> failwith "todo"
    | Nullable dataType -> failwith "todo"
    | Int32 -> failwith "todo"

type Field = {
    Name: string
    Type: DataType
}

let [<Literal>] nl = "\n"

let createTable tableName (fieldList: Field list) =
    if String.isNulOfWhiteSpace tableName then
        Error "Table name is empty."
    elif List.isEmpty fieldList then
        Error $"{nameof fieldList} is empty"
    else
        let sb = StringBuilder()
        sb.Append($"CREATE TABLE {tableName} ({nl}") |> ignore

        let padding = "    "
        let rec appendFields = function
            | [] -> ()
            | [ { Name = name; Type = typeObj } ] ->
                sb.Append($"{padding}`{name}` {dataTypeToString typeObj}{nl}") |> ignore
            | { Name = name; Type = typeObj } :: tail ->
                sb.Append($"{padding}`{name}` {dataTypeToString typeObj},{nl}") |> ignore
                appendFields tail
        appendFields fieldList

        sb.Append(")") |> ignore

        Ok (sb.ToString())

let [<Test>] ``creation of migration works properly``() =
    let createTableQuery = createTable "hello" [
        {Name = "trip_id"; Type = DataType.UInt32 }
        {Name = "name"; Type = DataType.String }
    ]
    let expect = """CREATE TABLE hello (
    `trip_id` UInt32,
    `name` String
)"""
    let expect = expect.Replace("\r", "")
    Assert.AreEqual(expect, Result.get createTableQuery)
