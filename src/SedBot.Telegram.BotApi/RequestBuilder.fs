[<RequireQualifiedAccess>]
module rec SedBot.Telegram.RequestBuilder

open System
open System.IO
open System.Linq.Expressions
open System.Net.Http
open System.Text
open SedBot.Common.TypeExtensions
open SedBot.Telegram.BotApi.Types.CoreTypes
open TypeShape.Core
open TypeShape.Core.Utils

#nowarn "64"

type internal Marker = interface end

type FieldName = string
type StreamField = FieldName * Stream

module Shape =
    let private SomeU = Some()
    let inline private test<'T> (s : TypeShape) =
        match s with
        | :? TypeShape<'T> -> SomeU
        | _ -> None

    let (|Stream|_|) s = test<Stream> s
    let (|HttpClient|_|) s = test<HttpClient> s
    let (|Exception|_|) s = test<Exception> s

let mkMemberPrinter (shape : IShapeMember<'DeclaringType>) (form: MultipartFormDataContent) =
   shape.Accept {
       new IMemberVisitor<'DeclaringType, 'DeclaringType -> (string * StreamField list)> with
           member _.Visit (shape : ShapeMember<'DeclaringType, 'Field>) =
               let fieldPrinter = build<'Field> form
               fieldPrinter << shape.Get
   }

let builderDynamic (t: Type) msg form =
    let reflectedFunc = typeof<Marker>.DeclaringType.GetMethod("buildExec").MakeGenericMethod(t)
    let build =
        Expression
            .Lambda<Func<MultipartFormDataContent -> IBotRequest -> (string * StreamField list)>>(Expression.Call(reflectedFunc))
            .Compile()
            .Invoke()
    build msg form

let buildExec<'T when 'T :> IBotRequest> () =
    let invokedBuild form (msg: IBotRequest) =
        build<'T> form (msg :?> 'IBotRequest)
    invokedBuild

let rec serialize<'T> (msg: 'T) =
    let ctx = new TypeGenerationContext()
    mkPrinterCached<'T> ctx None msg |> fst

let rec build<'T> (form: MultipartFormDataContent) (msg: 'T) : string * StreamField list =
    let ctx = new TypeGenerationContext()
    mkPrinterCached<'T> ctx (Some form) msg

and mkPrinterCached<'T> (ctx: TypeGenerationContext) (form: MultipartFormDataContent option) : 'T -> (string * StreamField list) =
    match ctx.InitOrGetCachedValue<'T -> (string * StreamField list)> (fun c t -> c.Value t) with
    | Cached(value = p) -> p
    | NotCached t ->
        let p = mkPrinterAux<'T> ctx form
        ctx.Commit t p

and mkPrinterAux<'T> (ctx: TypeGenerationContext) (form: MultipartFormDataContent option) : 'T -> (string * StreamField list) =
    let wrap(p : 'a -> (string * StreamField list)) = unbox<'T -> (string * StreamField list)> p

    let mkFieldPrinter (field: IShapeReadOnlyMember<'DeclaringType>) =
        field.Accept {
            new IReadOnlyMemberVisitor<'DeclaringType, string * ('DeclaringType -> (string * StreamField list))> with
                member _.Visit(field : ReadOnlyMember<'DeclaringType, 'Field>) =
                    let fieldPrinter = mkPrinterCached<'Field> ctx form
                    field.Label, field.Get >> fieldPrinter
        }

    let inline scf key (value: string) =
        let key = camelCaseToSnakeCase key
        match value with
        | null -> null
        | _ ->
            form |> Option.iter _.Add(new StringContent(value), key)
            $"\"{camelCaseToSnakeCase key}\": {value}"

    match shapeof<'T> with
    | Shape.FSharpFunc _
    | Shape.Exception
    | Shape.HttpClient -> fun _ -> null, []

    | Shape.Bool -> fun bool -> (if ucast<'T, bool> bool then "true" else "false"), []
    | Shape.Int32
    | Shape.Int64
    | Shape.Double -> fun number -> string number, []
    | Shape.String -> fun str -> ucast<'T, string> str, []

    | Shape.Uri -> fun uri -> string uri, []

    | Shape.Stream -> fun stream -> null, [null, ucast<'T, Stream> stream]

    | Shape.FSharpOption s ->
        s.Element.Accept {
            new ITypeVisitor<'T -> (string * StreamField list)> with
                member _.Visit<'a> () =
                    let tp = mkPrinterCached<'a> ctx form
                    wrap(function Some t -> tp t | None -> (null, []))
        }

    | Shape.FSharpList s ->
        s.Element.Accept {
            new ITypeVisitor<'T -> (string * StreamField list)> with
                member _.Visit<'a> () =
                    let tp = mkPrinterCached<'a> ctx form
                    wrap(
                        fun (ts: 'a list) ->
                                        ts
                                        |> Seq.fold (fun (storedString: StringBuilder, storedStreams: StreamField list) v ->
                                            let string, streams = tp v
                                            (storedString.Append(string), storedStreams @ streams)) ((StringBuilder(), []))
                                        |> fun (x, y) -> $"[{x}]", y
                    )
        }

    | Shape.Array s when s.Rank = 1 ->
        s.Element.Accept {
            new ITypeVisitor<'T -> (string * StreamField list)> with
                member _.Visit<'a> () =
                    let tp = mkPrinterCached<'a> ctx form
                    wrap(
                        fun (ts: 'a []) ->
                            ts
                            |> Seq.fold (fun (storedString: string list, streamFieldsAcc: StreamField list) v ->
                                let printedStr, streamFields = tp v
                                (storedString @ [printedStr], streamFieldsAcc @ streamFields)
                            ) (([], []))
                            |> fun (printedStr, streamFields) -> let printedStr = String.Join(",\n", printedStr) in $"[\n{printedStr}\n]", streamFields
                    )
        }

    | Shape.FSharpUnion (:? ShapeFSharpUnion<'T> as shape) ->
        let mkUnionCasePrinter (unionCaseShape: ShapeFSharpUnionCase<'T>) =
            let fieldPrinters = unionCaseShape.Fields |> Array.map mkFieldPrinter
            fun (u: 'T) ->
                match fieldPrinters with
                | [||] -> unionCaseShape.CaseInfo.Name, []
                | [| _, fieldPrinter |] -> fieldPrinter u
                | fps ->
                    fps
                    |> Seq.fold (fun (printedStrAcc: StringBuilder, streamFieldsAcc: StreamField list) (_, fp) ->
                        let printedStr, streamFields = fp u
                        let streamFields = streamFieldsAcc @ streamFields

                        if printedStr = null then
                            printedStrAcc, streamFields
                        elif printedStrAcc.Length = 0 then
                            printedStrAcc.Append(printedStr), streamFields
                        else
                            printedStrAcc.Append(printedStr).Append(","), streamFields) ((StringBuilder(), []))
                    |> fun (printedStr, streamField) ->
                        if List.isEmpty streamField then
                            printedStr, streamField
                        else
                            let stream = List.head streamField |> snd
                            printedStr, [ (printedStr.ToString(), stream) ]
                    |> fun (printedStr, streamFields) -> $"[ {printedStr} ]", streamFields

        let casePrinters = shape.UnionCases |> Array.map mkUnionCasePrinter
        fun (u: 'T) ->
            let printer = casePrinters[shape.GetTag u]
            printer u

    | Shape.FSharpRecord (:? ShapeFSharpRecord<'T> as shape) ->
        let fieldPrinters = shape.Fields |> Array.map mkFieldPrinter
        fun (r: 'T) ->
            fieldPrinters
            |> Seq.fold (fun (storedString: string list, storedStreams: StreamField list) (label, fieldPrinter) ->
                let value, streams = fieldPrinter r
                if streams.IsEmpty then
                    let printedField = scf label value
                    if printedField = null then
                        storedString, storedStreams @ streams
                    else
                        storedString @ [ printedField ], storedStreams @ streams
                else
                    let fileName, stream = List.head streams
                    form |> Option.iter _.Add(new StreamContent(stream), camelCaseToSnakeCase label, fileName)
                    storedString, storedStreams
            ) (([], []))
            |> fun (printedFields, streamFields) -> let printedFields = String.Join(", ", printedFields) in $"{{ {printedFields} }}", streamFields

    | Shape.Poco (:? ShapePoco<'T> as shape) ->
        let propPrinters = shape.Properties |> Array.map mkFieldPrinter
        fun (fieldType: 'T) ->
            propPrinters
            |> Seq.fold (fun (storedString: StringBuilder, storedStreams: StreamField list) (label, fieldPrinter) ->
                let value, streams = fieldPrinter fieldType
                (storedString.Append(scf label value).Append("\n "), storedStreams @ streams)
            ) ((StringBuilder(), []))
            |> fun (printedStr, streamFields) ->
                $"{{ {printedStr} }}", streamFields

    | _ -> failwithf "unsupported type '%O'" typeof<'T>
