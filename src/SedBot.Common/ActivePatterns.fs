module SedBot.Common.ActivePatterns

open System

module String =
    let inline (|IsNullOrWhitespace|_|) a =
        String.isNullOfWhiteSpace a |> Option.ofBool

    let inline (|Contains|_|) (str: string) (v: string) =
        str.Contains(v) |> Option.ofBool

    let inline (|EqualsInvariantCultureIgnoreCase|_|) (pattern: string) (str: string) =
        str.Equals(pattern, StringComparison.InvariantCultureIgnoreCase) |> Option.ofBool
