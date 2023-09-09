module SedBot.Common.ActivePatterns

module String =
    let (|IsNullOrWhitespace|_|) a =
        String.isNullOfWhiteSpace a |> Option.ofBool

    let (|Contains|_|) (str: string) (v: string) =
        str.Contains(v) |> Option.ofBool
