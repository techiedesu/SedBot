[<RequireQualifiedAccess; AutoOpen>]
module rec SedBot.Json.SedJsonSerializer

open System
open System.IO
open System.Net.Http
open System.Text
open SedBot.Common.TypeExtensions
open TypeShape.Core
open TypeShape.Core.Utils

let toHex (v: int) =
    if v <= 9 then
        v + 48 |> char
    else
        v + 87 |> char

let escapeUnicode (c: char) =
    [|
        let c = int c

        yield '\\'
        yield 'u'
        yield (c >>> 12) &&& 15 |> toHex
        yield (c >>> 08) &&& 15 |> toHex
        yield (c >>> 04) &&& 15 |> toHex
        yield c          &&& 15 |> toHex
    |] |> String

let escapeJsonString (str: string) =
    if str = null || str.Length = 0 then
        str
    else
        let sb = StringBuilder()
        let sbWrite : string -> unit = sb.Append >> ignore
        let sbWriteC : char -> unit =  sb.Append >> ignore

        let rec loop (position: int) =
            if String.length str = position then
                string sb
            else
                let c = str[position]
                match c with
                | '\t' -> sbWrite @"\t"
                | '\n' -> sbWrite @"\n"
                | '\r' -> sbWrite @"\r"
                | '\f' -> sbWrite @"\f"
                | '\b' -> sbWrite @"\b"
                | '\\' -> sbWrite @"\\"
                | '\u0085' -> sbWrite @"\u0085"
                | '\u2028' -> sbWrite @"\u2028"
                | '\u2029' -> sbWrite @"\u2029"
                | '\'' -> sbWrite @"\'"
                | '"' -> sbWrite "\\\""
                | _ when c <= '\u001f' -> escapeUnicode c |> sbWrite
                | c -> sbWriteC c

                loop (position + 1)
        loop 0

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

type JsonSettings ={
    Minimized: bool
} with
    static member Empty = {
        Minimized = true
    }

/// Converts .NET type to string JSON representation (aka serialize)
/// Trying to create human-readable representation
let serialize<'T> (msg: 'T) =
    let ctx = new TypeGenerationContext()
    let settings = { Minimized = false }
    mkPrinterCached<'T> ctx settings msg

/// Converts .NET type to string JSON representation (aka serialize)
/// Minimized representation
let serializeMinimized<'T> (msg: 'T) =
    let ctx = new TypeGenerationContext()
    let settings = JsonSettings.Empty
    mkPrinterCached<'T> ctx settings msg

let rec build<'T> (msg: 'T) : string =
    let ctx = new TypeGenerationContext()
    let settings = JsonSettings.Empty
    mkPrinterCached<'T> ctx settings msg

and mkPrinterCached<'T> (ctx: TypeGenerationContext) (settings: JsonSettings) : 'T -> (string) =
    match ctx.InitOrGetCachedValue<'T -> string> (fun c t -> c.Value t) with
    | Cached(value = p) -> p
    | NotCached t ->
        let p = mkPrinterAux<'T> ctx settings
        ctx.Commit t p

and mkPrinterAux<'T> (ctx: TypeGenerationContext) (settings: JsonSettings) : 'T -> (string) =
    let wrap(p : 'a -> (string)) = unbox<'T -> (string)> p
    let nl = if settings.Minimized then "" else "\n"

    let mkFieldPrinter (field: IShapeReadOnlyMember<'DeclaringType>) =
        field.Accept {
            new IReadOnlyMemberVisitor<'DeclaringType, string * ('DeclaringType -> (string))> with
                member _.Visit(field : ReadOnlyMember<'DeclaringType, 'Field>) =
                    let fieldPrinter = mkPrinterCached<'Field> ctx
                    field.Label, field.Get >> fieldPrinter settings
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
    | Shape.Stream
    | Shape.HttpClient -> fun _ -> null

    | Shape.Bool -> fun bool -> (if ucast<'T, bool> bool then "true" else "false")
    | Shape.Int32
    | Shape.Int64
    | Shape.Double -> string
    | Shape.String -> fun str -> $"\"{ucast<'T, string> str |> escapeJsonString}\""

    | Shape.Uri -> string

    | Shape.FSharpOption s ->
        s.Element.Accept {
            new ITypeVisitor<'T -> (string)> with
                member _.Visit<'a> () =
                    let tp = mkPrinterCached<'a> ctx settings
                    wrap(function Some t -> tp t | None -> (null))
        }

    | Shape.FSharpList s ->
        s.Element.Accept {
            new ITypeVisitor<'T -> (string)> with
                member _.Visit<'a> () =
                    let tp = mkPrinterCached<'a> ctx settings
                    wrap(
                        fun (ts: 'a list) ->
                                        ts
                                        |> Seq.fold (fun (storedString: StringBuilder) v -> storedString.Append(tp v)) (StringBuilder())
                                        |> fun (x) -> $"[{x}]"
                    )
        }

    | Shape.Array s when s.Rank = 1 ->
        s.Element.Accept {
            new ITypeVisitor<'T -> (string)> with
                member _.Visit<'a> () =
                    let tp = mkPrinterCached<'a> ctx settings
                    wrap(
                        fun (ts: 'a []) ->
                            ts
                            |> Seq.fold (fun (storedString: string list) v ->
                                let printedStr = tp v
                                (storedString @ [printedStr])
                            ) ([])
                            |> fun (printedStr) -> let printedStr = String.Join($",{nl}", printedStr) in $"[{nl}{printedStr}{nl}]"
                    )
        }

    | Shape.FSharpUnion (:? ShapeFSharpUnion<'T> as shape) ->
        let mkUnionCasePrinter (unionCaseShape: ShapeFSharpUnionCase<'T>) =
            let fieldPrinters = unionCaseShape.Fields |> Array.map mkFieldPrinter
            fun (u: 'T) ->
                match fieldPrinters with
                | [||] -> unionCaseShape.CaseInfo.Name |> sprintf "\"%s\""
                | [| _, fieldPrinter |] -> fieldPrinter u
                | fps ->
                    fps
                    |> Seq.map (fun (_, fp) -> fp u)
                    |> Seq.filter ((<>) null)
                    |> String.concat ", "
                    |> sprintf "[ %s ]"

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

            ) []
            |> fun (printedFields) -> let printedFields = String.Join(", ", printedFields) in $"{{ {printedFields} }}"

    | Shape.Poco (:? ShapePoco<'T> as shape) ->
        let propPrinters = shape.Properties |> Array.map mkFieldPrinter
        fun (fieldType: 'T) ->
            propPrinters
            |> Seq.fold (fun (storedString: StringBuilder) (label, fieldPrinter) ->
                let value = fieldPrinter fieldType
                storedString.Append(scf label value).Append($"{nl} ")
            ) (StringBuilder())
            |> fun (printedStr) ->
                $"{{ {printedStr} }}"

    | _ -> failwithf "unsupported type '%O'" typeof<'T>
