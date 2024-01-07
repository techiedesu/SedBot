[<AutoOpen>]
module rec SedBot.Telegram.RequestBuilder

open System
open System.Linq.Expressions
open System.Net.Http
open SedBot.Common.TypeExtensions
open SedBot.Telegram.BotApi.Types.CoreTypes
open TypeShape.Core
open TypeShape.Core.Utils

type internal Marker = interface end

let mkMemberPrinter (shape : IShapeMember<'DeclaringType>) (form: MultipartFormDataContent) =
   shape.Accept {
       new IMemberVisitor<'DeclaringType, 'DeclaringType -> string> with
           member _.Visit (shape : ShapeMember<'DeclaringType, 'Field>) =
               let fieldPrinter = build<'Field> form
               fieldPrinter << shape.Get
   }

let builderDynamic (t: Type) msg form =
    let reflectedFunc =typeof<Marker>.DeclaringType.GetMethod("buildExec").MakeGenericMethod(t)
    let build = Expression.Lambda<Func<MultipartFormDataContent -> IBotRequest -> string>>(Expression.Call(reflectedFunc)).Compile().Invoke()
    build msg form

#nowarn "64"

let buildExec<'T when 'T :> IBotRequest> () =
    let x form (msg: IBotRequest) =
        build<'T> form (msg :?> 'IBotRequest)
    x

let rec build<'T> (form: MultipartFormDataContent) (msg: 'T) : string =
    let ctx = new TypeGenerationContext()
    mkPrinterCached<'T> ctx form msg

and mkPrinterCached<'T> (ctx : TypeGenerationContext) (form: MultipartFormDataContent) : 'T -> string =
    match ctx.InitOrGetCachedValue<'T -> string> (fun c t -> c.Value t) with
    | Cached(value = p) -> p
    | NotCached t ->
        let p = mkPrinterAux<'T> ctx form
        ctx.Commit t p

and mkPrinterAux<'T> (ctx : TypeGenerationContext) (form: MultipartFormDataContent) : 'T -> string =
    let wrap(p : 'a -> string) = unbox<'T -> #obj> p

    let mkFieldPrinter (field : IShapeReadOnlyMember<'DeclaringType>) =
        field.Accept {
            new IReadOnlyMemberVisitor<'DeclaringType, string * ('DeclaringType -> string)> with
                member _.Visit(field : ReadOnlyMember<'DeclaringType, 'Field>) =
                    let fp = mkPrinterCached<'Field> ctx form
                    field.Label, fp << field.Get
        }

    let inline scf k v = let r = $"{k} = {v}" in match v with null -> r | _ -> new StringContent(r) |> form.Add; r

    match shapeof<'T> with
    | Shape.Bool
    | Shape.Int32
    | Shape.Int64
    | Shape.String -> string

    | Shape.FSharpOption s ->
        s.Element.Accept {
            new ITypeVisitor<'T -> string> with
                member _.Visit<'a> () = // 'T = 'a option
                    let tp = mkPrinterCached<'a> ctx form
                    wrap(function Some t -> sprintf "%s" (tp t) | None -> null)
        }

    | Shape.FSharpList s ->
        s.Element.Accept {
            new ITypeVisitor<'T -> string> with
                member _.Visit<'a> () = // 'T = 'a list
                    let tp = mkPrinterCached<'a> ctx form
                    wrap(fun (ts : 'a list) -> ts |> Seq.map tp |> String.concat "; " |> sprintf "[%s]")
        }

    | Shape.Array s when s.Rank = 1 ->
        s.Element.Accept {
            new ITypeVisitor<'T -> string> with
                member _.Visit<'a> () = // 'T = 'a []
                    let tp = mkPrinterCached<'a> ctx form
                    wrap(fun (ts : 'a []) -> ts |> Seq.map tp |> String.concat ", " |> sprintf "[%s]")
        }

    | Shape.FSharpUnion (:? ShapeFSharpUnion<'T> as shape) ->
        let mkUnionCasePrinter (s: ShapeFSharpUnionCase<'T>) =
            let fieldPrinters = s.Fields |> Array.map mkFieldPrinter
            fun (u:'T) ->
                match fieldPrinters with
                | [||] -> s.CaseInfo.Name
                | [| _, fieldPrinter |] -> sprintf "%s %s" s.CaseInfo.Name (fieldPrinter u)
                | fps ->
                    fps
                    |> Seq.map (fun (_, fp) -> fp u)
                    |> String.concat ", "
                    |> sprintf "%s(%s)" s.CaseInfo.Name

        let casePrinters = shape.UnionCases |> Array.map mkUnionCasePrinter
        fun (u: 'T) ->
            let printer = casePrinters[shape.GetTag u]
            printer u

    | Shape.FSharpRecord (:? ShapeFSharpRecord<'T> as shape) ->
        let fieldPrinters = shape.Fields |> Array.map mkFieldPrinter
        fun (r: 'T) ->
            fieldPrinters
            |> Seq.map ^ fun (label, ep) -> let value = ep r in scf label value
            |> String.concat "\n "
            |> sprintf "{ %O }"

    | Shape.Poco (:? ShapePoco<'T> as shape) ->
        let propPrinters = shape.Properties |> Array.map mkFieldPrinter
        fun (r: 'T) ->
            propPrinters
            |> Seq.map ^ fun (label, ep) -> let value = ep r in scf label value
            |> String.concat "\n  "
            |> sprintf "{ %s }"

    | _ -> failwithf "unsupported type '%O'" typeof<'T>
