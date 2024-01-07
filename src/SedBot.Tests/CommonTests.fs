module SedBot.Tests.CommonTests

open SedBot.Common.TypeExtensions

open NUnit.Framework

[<Test>]
let ``Option and ValueOption detect works properly`` () =
    if isOptionType typeof<obj> then Assert.Fail("No. It isn't Option type")
    if isValueOptionType typeof<obj> then Assert.Fail("No. It isn't ValueOption type")

    if isOptionType typeof<Option<obj>> |> not then Assert.Fail("No. It's Option type")
    if isValueOptionType typeof<ValueOption<obj>> |> not then Assert.Fail("No. It's ValueOption type")

[<TestCase("snack_case", "SnackCase")>]
[<TestCase("CamelCase", "CamelCase")>]
[<TestCase("S", "S")>]
[<TestCase("s", "S")>]
[<TestCase("", "")>]
let ``Snack case to camel case`` (str, expected) =
    Assert.That (snackCaseToCamelCase str, Is.EqualTo expected)

[<TestCase("snack_case", "snack_case")>]
[<TestCase("CamelCase", "camel_case")>]
[<TestCase("S", "s")>]
[<TestCase("s", "s")>]
[<TestCase("", "")>]
let ``Camel case to snack case`` (str, expected) =
    Assert.That (camelCaseToSnakeCase str, Is.EqualTo expected)
