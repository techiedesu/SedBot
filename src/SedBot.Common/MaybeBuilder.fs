module SedBot.Common.MaybeBuilder

open System

[<Sealed>]
type MaybeBuilder() =
    member inline _.Bind(optionValue, f) =
        match optionValue with
        | None -> None
        | Some value -> f value

    member inline _.Return maybeNull =
       if Object.ReferenceEquals(maybeNull, null) then
           None
       else
           Some maybeNull

    member inline _.ReturnFrom (optionValue: 'a option) = optionValue

    member inline _.Combine(optionValue, f) =
        match optionValue with
        | Some _ -> optionValue
        | _ -> f()

    member inline _.Delay f = f

    member inline _.Run f = f()

    member inline _.Zero() = None
    member inline _.TryWith(expr, handler) =
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
