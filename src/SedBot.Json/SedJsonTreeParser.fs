[<RequireQualifiedAccess>]
module SedBot.Json.SedJsonTreeParser

open System.Runtime.CompilerServices
open System.Text.RegularExpressions
open FParsec
open SedBot.Common.TypeExtensions

[<assembly: InternalsVisibleTo("SedBot.Json.Tests")>]
do()

type JsonValue =
    | String of string
    | Number of string
    | Null
    | Bool of bool
    | Object of Map<string, JsonValue>
    | Array of JsonValue list

let internal unescapeStr (str: string) = Regex.Unescape(str)

let internal createParseTree handleName : Parser<JsonValue, unit> =
    let skipSpacesAndNewLines = optional (many (anyOf [ ' '; '\n'; '\r'; '\t' ]))

    let typeSelector, typeSelectorRef = createParserForwardedToRef<JsonValue, unit>()

    let str = between
                (pchar '"')
                (pchar '"')
                (manyChars (noneOf "\""))

    let key = str |>> handleName
    let tStr = str |>> (unescapeStr >> JsonValue.String)

    let tNumber =
        let sign = opt (pchar '-' <|> pchar '+') .>>. many1Chars digit
        let number = opt (pchar '.' >>. manyChars digit)
        let toStr (v: _ option) = match v with None -> "" | Some v -> $".{v}"
        let ofObj v = match v with | None -> "" | Some v -> string v
        let numberParser = pipe2 sign number (fun (sign, int) frac -> $"{ofObj sign}{int}{toStr frac}")
        numberParser .>> skipSpacesAndNewLines |>> JsonValue.Number

    let tBool =
        (pstringCI "true" >>% true) <|> (pstringCI "false" >>% false) .>> skipSpacesAndNewLines
        |>> JsonValue.Bool

    let tNull = pstringCI "null" .>> skipSpacesAndNewLines >>% JsonValue.Null

    let tArray = between
                    (pchar '[' .>> skipSpacesAndNewLines)
                    (pchar ']' .>> skipSpacesAndNewLines)
                    (sepBy typeSelector (pchar ',' .>> skipSpacesAndNewLines))
                 |>> JsonValue.Array

    let keyValue = pipe2
                    (key .>> pchar ':' .>> skipSpacesAndNewLines)
                    (skipSpacesAndNewLines >>. typeSelector .>> skipSpacesAndNewLines)
                    (fun key value -> key, value)

    let tObj = between
                (pchar '{' .>> skipSpacesAndNewLines)
                (pchar '}' .>> skipSpacesAndNewLines)
                (sepBy keyValue (pchar ',' .>> skipSpacesAndNewLines))
                |>> (Map.ofList >> JsonValue.Object)

    typeSelectorRef.Value <- tStr
        <|> tBool
        <|> tNull
        <|> tNumber
        <|> tObj
        <|> tArray

    skipSpacesAndNewLines
    >>. typeSelector
    .>> skipSpacesAndNewLines

let parse (input: string) (toCamelCase: bool) =
    let handleName =
        if toCamelCase then snackCaseToCamelCase
        else id

    match run (createParseTree handleName) input with
    | Success (result, _, _position) ->
        Some result
    | Failure (errorMsg, _, _) ->
        printfn $"Ошибка парсинга: {errorMsg}"
        None
