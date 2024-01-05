module SedBot.Json.SedJsonDeserializer

open System
open System.Runtime.CompilerServices
open Microsoft.FSharp.Reflection
open SedBot.Json.SedNetTreeTypeBuilder
open SedBot.Common.TypeExtensions

let wrapObjAux (t: Type) (o: obj) : obj =
    let isOption = isOptionType t
    let isValueOption = isValueOptionType t

    if isOption then
        if o = null then
            box ^ None
        else
            box ^ Some o
    elif isValueOption then
        if o = null then
            box ^ ValueNone
        else
            box ^ ValueSome o
    else
        o |> box

let optTypeWrapperAux<'T> isOption isValueOption (v: 'T) : obj =
    if isOption then
        v |> Some |> box
    elif isValueOption then
        v |> ValueSome |> box
    else
        v |> box

let private wrapValueAux (rawNumber: string) (t: Type) : obj =
    let isOption = isOptionType t
    let isValueOption = isValueOptionType t
    let t =
        if isOption || isValueOption then
            t.GenericTypeArguments[0]
        else
            t

    match t.Name with
    | nameof Int64 ->
        Int64.Parse
            rawNumber |> optTypeWrapperAux isOption isValueOption
    | nameof UInt64 ->
        UInt64.Parse
            rawNumber |> optTypeWrapperAux isOption isValueOption
    | nameof Int32 ->
        Int32.Parse
            rawNumber |> optTypeWrapperAux isOption isValueOption
    | nameof UInt32 ->
        UInt32.Parse
            rawNumber |> optTypeWrapperAux isOption isValueOption
    | nameof Int16 ->
        Int16.Parse
            rawNumber |> optTypeWrapperAux isOption isValueOption
    | nameof UInt16 ->
        UInt16.Parse
            rawNumber |> optTypeWrapperAux isOption isValueOption
    | nameof Double ->
        Double.Parse
            rawNumber |> optTypeWrapperAux isOption isValueOption
    | nameof SByte ->
        SByte.Parse
            rawNumber |> optTypeWrapperAux isOption isValueOption
    | nameof Byte ->
        Byte.Parse
            rawNumber |> optTypeWrapperAux isOption isValueOption
    | name ->
        failwith $"Can't handle {name} as number"

/// Try to convert Number to concrete Type
let private mapNumberToObjectAux typeNode n =
    wrapValueAux n typeNode.Type

let jsonToTypeMapper (rootJsonNode: SedJsonTreeParser.JsonValue) (rootTypeNode: Node) =
    let rec loop (jsonNode: SedJsonTreeParser.JsonValue) (typeNode: Node) =
        let getTypeNodeByFieldName propName = treeTypeWalker propName typeNode

        // Wrap with option if needed
        let optWrap (object: obj) = wrapObjAux typeNode.Type object

        match jsonNode with
        | SedJsonTreeParser.JsonValue.Null ->
            null |> optWrap

        | SedJsonTreeParser.JsonValue.String s ->
            s |> optWrap

        | SedJsonTreeParser.JsonValue.Number n ->
            mapNumberToObjectAux typeNode n

        | SedJsonTreeParser.JsonValue.Bool b ->
            b |> optWrap

        | SedJsonTreeParser.JsonValue.Object members ->
            let processField (fsharpPropInfo: Reflection.PropertyInfo) =
                let fieldName = fsharpPropInfo.Name
                let newNode = getTypeNodeByFieldName fieldName

                let execLoop jsonValue =
                    let isOption = isOptionType newNode.Type
                    let isValueOption = isValueOptionType newNode.Type

                    if isOption then
                        let actualNode = newNode.Children |> Seq.head
                        let res = loop jsonValue actualNode
                        Activator.CreateInstance(newNode.Type, res)
                    elif isValueOption then
                        let actualNode = newNode.Children |> Seq.head
                        let res = loop jsonValue actualNode
                        if res = null then
                            ValueNone |> box
                        else
                            let uCases = FSharpType.GetUnionCases(newNode.Type)[1]
                            FSharpValue.MakeUnion(uCases, [| res |])
                    else
                        loop jsonValue newNode

                Map.tryFind fieldName members
                |> Option.map execLoop
                |> Option.defaultValue null

            let recordFields =
                typeNode.Type
                |> FSharpType.GetRecordFields
                |> Array.map processField
            FSharpValue.MakeRecord(typeNode.Type, recordFields)

        | SedJsonTreeParser.JsonValue.Array items ->
            let firstChild = Seq.head typeNode.Children
            let els = items
                        |> List.map (fun item ->
                            loop item firstChild)
                        |> Array.ofList

            let res = Array.CreateInstance(firstChild.Type, els.Length)
            Array.Copy(els, res, els.Length)
            res |> box

    loop rootJsonNode rootTypeNode

let deserialize<'T> toCamelCase (json: string) : 'T =
    let rootNode = buildTypeTree typeof<'T> |> snd
    match SedJsonTreeParser.parse json toCamelCase with
    | Some json ->
         let res = jsonToTypeMapper json rootNode
         res :?> 'T
    | None ->
        failwith "JSON PARSE FAIL"

let dynDeserialize t toCamelCase (json: string) =
    let rootNode = buildTypeTree t |> snd
    match SedJsonTreeParser.parse json toCamelCase with
    | Some json ->
        jsonToTypeMapper json rootNode
    | None ->
        failwith "JSON PARSE FAIL"
