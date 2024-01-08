[<RequireQualifiedAccess; AutoOpen>]
module rec SedBot.Json.SedJsonSerializer

open System
open System.IO
open System.Net.Http
open System.Text
open SedBot.Common.TypeExtensions
open TypeShape.Core
open TypeShape.Core.Utils

#nowarn "64"

type internal Marker = interface end

type FieldName = string

module Shape =
    let private SomeU = Some()
    let inline private test<'T> (s : TypeShape) =
        match s with
        | :? TypeShape<'T> -> SomeU
        | _ -> None

    let (|Stream|_|) s = test<Stream> s
    let (|HttpClient|_|) s = test<HttpClient> s
    let (|Exception|_|) s = test<Exception> s

let mkMemberPrinter (shape : IShapeMember<'DeclaringType>) =
   shape.Accept {
       new IMemberVisitor<'DeclaringType, 'DeclaringType -> (string)> with
           member _.Visit (shape : ShapeMember<'DeclaringType, 'Field>) =
               let fieldPrinter = build<'Field>
               fieldPrinter << shape.Get
   }

let rec serialize<'T> (msg: 'T) =
    let ctx = new TypeGenerationContext()
    mkPrinterCached<'T> ctx msg

let rec build<'T> (msg: 'T) : string =
    let ctx = new TypeGenerationContext()
    mkPrinterCached<'T> ctx msg

and mkPrinterCached<'T> (ctx: TypeGenerationContext) : 'T -> (string) =
    match ctx.InitOrGetCachedValue<'T -> string> (fun c t -> c.Value t) with
    | Cached(value = p) -> p
    | NotCached t ->
        let p = mkPrinterAux<'T> ctx
        ctx.Commit t p

and mkPrinterAux<'T> (ctx: TypeGenerationContext) : 'T -> (string) =
    let wrap(p : 'a -> (string)) = unbox<'T -> (string)> p

    let mkFieldPrinter (field: IShapeReadOnlyMember<'DeclaringType>) =
        field.Accept {
            new IReadOnlyMemberVisitor<'DeclaringType, string * ('DeclaringType -> (string))> with
                member _.Visit(field : ReadOnlyMember<'DeclaringType, 'Field>) =
                    let fieldPrinter = mkPrinterCached<'Field> ctx
                    field.Label, field.Get >> fieldPrinter
        }

    let inline scf key (value: string) =
        let key = camelCaseToSnakeCase key
        match value with
        | null -> null
        | _ ->
            $"\"{camelCaseToSnakeCase key}\": {value}"

    match shapeof<'T> with
    | Shape.FSharpFunc _
    | Shape.Exception
    | Shape.HttpClient -> fun _ -> null

    | Shape.Bool -> fun bool -> (if ucast<'T, bool> bool then "true" else "false")
    | Shape.Int32
    | Shape.Int64
    | Shape.Double -> string
    | Shape.String -> fun str -> $"\"{str}\""

    | Shape.Uri -> string

    | Shape.Stream -> fun _ -> null

    | Shape.FSharpOption s ->
        s.Element.Accept {
            new ITypeVisitor<'T -> (string)> with
                member _.Visit<'a> () =
                    let tp = mkPrinterCached<'a> ctx
                    wrap(function Some t -> tp t | None -> (null))
        }

    | Shape.FSharpList s ->
        s.Element.Accept {
            new ITypeVisitor<'T -> (string)> with
                member _.Visit<'a> () =
                    let tp = mkPrinterCached<'a> ctx
                    wrap(
                        fun (ts: 'a list) ->
                                        ts
                                        |> Seq.fold (fun (storedString: StringBuilder) v ->
                                            let string = tp v
                                            storedString.Append(string)) (StringBuilder())
                                        |> fun (x) -> $"[{x}]"
                    )
        }

    | Shape.Array s when s.Rank = 1 ->
        s.Element.Accept {
            new ITypeVisitor<'T -> (string)> with
                member _.Visit<'a> () =
                    let tp = mkPrinterCached<'a> ctx
                    wrap(
                        fun (ts: 'a []) ->
                            ts
                            |> Seq.fold (fun (storedString: string list) v ->
                                let printedStr = tp v
                                (storedString @ [printedStr])
                            ) ([])
                            |> fun (printedStr) -> let printedStr = String.Join(",\n", printedStr) in $"[\n{printedStr}\n]"
                    )
        }

    | Shape.FSharpUnion (:? ShapeFSharpUnion<'T> as shape) ->
        let mkUnionCasePrinter (unionCaseShape: ShapeFSharpUnionCase<'T>) =
            let fieldPrinters = unionCaseShape.Fields |> Array.map mkFieldPrinter
            fun (u: 'T) ->
                match fieldPrinters with
                | [||] -> unionCaseShape.CaseInfo.Name
                | [| _, fieldPrinter |] -> fieldPrinter u
                | fps ->
                    fps
                    |> Seq.fold (fun (printedStrAcc: StringBuilder) (_, fp) ->
                        let printedStr = fp u

                        if printedStr = null then
                            printedStrAcc
                        elif printedStrAcc.Length = 0 then
                            printedStrAcc.Append(printedStr)
                        else
                            printedStrAcc.Append(printedStr).Append(",")) ((StringBuilder()))
                    |> fun (printedStr) -> $"[ {printedStr} ]"

        let casePrinters = shape.UnionCases |> Array.map mkUnionCasePrinter
        fun (u: 'T) ->
            let printer = casePrinters[shape.GetTag u]
            printer u

    | Shape.FSharpRecord (:? ShapeFSharpRecord<'T> as shape) ->
        let fieldPrinters = shape.Fields |> Array.map mkFieldPrinter
        fun (r: 'T) ->
            fieldPrinters
            |> Seq.fold (fun (storedString: string list) (label, fieldPrinter) ->
                let value = fieldPrinter r
                let printedField = scf label value
                if printedField = null then
                    storedString
                else
                    storedString @ [ printedField ]

            ) (([]))
            |> fun (printedFields) -> let printedFields = String.Join(", ", printedFields) in $"{{ {printedFields} }}"

    | Shape.Poco (:? ShapePoco<'T> as shape) ->
        let propPrinters = shape.Properties |> Array.map mkFieldPrinter
        fun (fieldType: 'T) ->
            propPrinters
            |> Seq.fold (fun (storedString: StringBuilder) (label, fieldPrinter) ->
                let value = fieldPrinter fieldType
                storedString.Append(scf label value).Append("\n ")
            ) (StringBuilder())
            |> fun (printedStr) ->
                $"{{ {printedStr} }}"

    | _ -> failwithf "unsupported type '%O'" typeof<'T>
