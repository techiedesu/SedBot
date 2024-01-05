module SedBot.Tests.CommonTests

open SedBot.Common.TypeExtensions

open NUnit.Framework

[<Test>]
let ``Option and ValueOption detect works properly`` () =
    if isOptionType typeof<obj> then Assert.Fail("No. It isn't Option type")
    if isValueOptionType typeof<obj> then Assert.Fail("No. It isn't ValueOption type")

    if isOptionType typeof<Option<obj>> |> not then Assert.Fail("No. It's Option type")
    if isValueOptionType typeof<ValueOption<obj>> |> not then Assert.Fail("No. It's ValueOption type")
