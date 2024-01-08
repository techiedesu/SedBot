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
                    let fp = mkPrinterCached<'Field> ctx form
                    field.Label, field.Get >> fp
        }

    let inline scf key (value: string) =
        let key = camelCaseToSnakeCase key
        match value with
        | null ->
            null
        | _ ->
            form |> Option.iter _.Add(new StringContent(value), key)
            $"\"{camelCaseToSnakeCase key}\": \"{value}\""

    match shapeof<'T> with
    | Shape.FSharpFunc _
    | Shape.Exception _
    | Shape.HttpClient _ ->
        fun _ -> null, []

    | Shape.Bool -> fun x -> x.ToString().ToLowerInvariant(), []
    | Shape.Int32
    | Shape.Int64
    | Shape.Double
    | Shape.String -> fun x -> string x, []

    | Shape.Uri ->
        fun x -> string x, []

    | Shape.Stream ->
        fun x -> null, [null, ucast<'T, Stream> x]

    | Shape.FSharpOption s ->
        s.Element.Accept {
            new ITypeVisitor<'T -> (string * StreamField list)> with
                member _.Visit<'a> () = // 'T = 'a option
                    let tp = mkPrinterCached<'a> ctx form
                    wrap(function Some t -> tp t | None -> (null, []))
        }

    | Shape.FSharpList s ->
        s.Element.Accept {
            new ITypeVisitor<'T -> (string * StreamField list)> with
                member _.Visit<'a> () = // 'T = 'a list
                    let tp = mkPrinterCached<'a> ctx form
                    wrap(
                        fun (ts : 'a list) ->
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
                member _.Visit<'a> () = // 'T = 'a []
                    let tp = mkPrinterCached<'a> ctx form
                    wrap(
                        fun (ts: 'a []) ->
                            ts
                            |> Seq.fold (fun (storedString: string list, storedStreams: StreamField list) v ->
                                let string, streams = tp v
                                (storedString @ [string], storedStreams @ streams)
                            ) (([], []))
                            |> fun (x, y) -> let x = String.Join(",\n", x) in $"[\n{x}\n]", y
                    )
        }

    | Shape.FSharpUnion (:? ShapeFSharpUnion<'T> as shape) ->
        let mkUnionCasePrinter (s: ShapeFSharpUnionCase<'T>) =
            let fieldPrinters = s.Fields |> Array.map mkFieldPrinter
            fun (u:'T) ->
                match fieldPrinters with
                | [||] -> s.CaseInfo.Name |> fun x -> x, []
                | [| _, fieldPrinter |] -> (fieldPrinter u)
                | fps ->
                    fps
                    |> Seq.fold (fun (storedString: StringBuilder, storedStreams: StreamField list) (_, fp) ->
                        let (string, streams) = fp u

                        if string = null then
                            (storedString, storedStreams @ streams)
                        elif storedString.Length = 0 then
                            (storedString.Append(string), storedStreams @ streams)
                        else
                            (storedString.Append(string).Append(","), storedStreams @ streams)
                    ) ((StringBuilder(), []))
                    |> fun (x, streamField) ->
                        if List.isEmpty streamField then
                            x, streamField
                        else
                            let stream = List.head streamField |> snd
                            x, [ (x.ToString(), stream) ]
                    |> fun (x, y) -> $"[ {x} ]", y

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
                if streams.IsEmpty |> not then
                    let fileName, stream = List.head streams
                    form |> Option.iter _.Add(new StreamContent(stream), camelCaseToSnakeCase label, fileName)
                    (storedString, storedStreams)
                else
                    (storedString @ [ (scf label value) ], storedStreams @ streams)
            ) (([], []))
            |> fun (x, y) -> let x = String.Join(", ", Seq.filter ((<>) null) x) in $"{{ {x} }}", y

    | Shape.Poco (:? ShapePoco<'T> as shape) ->
        let propPrinters = shape.Properties |> Array.map mkFieldPrinter
        fun (r: 'T) ->
            propPrinters
            |> Seq.fold (fun (storedString: StringBuilder, storedStreams: StreamField list) (label, fieldPrinter) ->
                let value, streams = fieldPrinter r
                (storedString.Append(scf label value).Append("\n "), storedStreams @ streams)
            ) ((StringBuilder(), []))
            |> fun (x, y) ->
                $"{{ {x} }}", y

    | _ -> failwithf "unsupported type '%O'" typeof<'T>
