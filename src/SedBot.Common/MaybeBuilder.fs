module SedBot.Common.MaybeBuilder

open System

[<Sealed>]
type MaybeBuilder() =
    static member inline Bind(optionValue, f) =
        match optionValue with
        | None -> None
        | Some value -> f value

    static member inline Return maybeNull =
       if Object.ReferenceEquals(maybeNull, null) then
           None
       else
           Some maybeNull

    static member inline ReturnFrom (optionValue: 'a option) = optionValue

    static member inline Combine(optionValue, f) =
        match optionValue with
        | Some _ -> optionValue
        | _ -> f()

    static member inline Delay f = f

    static member inline Run f = f()

    static member inline Zero() = None
    static member inline TryWith(expr, handler) =
        try
            expr()
        with
        | ex -> handler ex

let maybe = MaybeBuilder()

module MaybeBuilderAnyReferenceTypeEx =
     type MaybeBuilder with
        member inline _.Bind(maybeNull, f) =
            if Object.ReferenceEquals(maybeNull, null) then
                None
            else
                f maybeNull
