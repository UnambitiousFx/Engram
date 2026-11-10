using System.Text.Json.Serialization;
using UnambitiousFx.Engram;

// The sample of RFC 8785 §3.2.4: the same data, written with whitespace, escapes and unusual numbers.
const string json =
    "{\"numbers\": [333333333.33333329, 1E30, 4.50, 2e-3, 0.000000000000000000000000001],"
    + " \"string\": \"\\u20ac$\\u000F\\u000aA'\\u0042\\u0022\\u005c\\\\\\\"\\/\","
    + " \"literals\": [null, true, false]}";

Console.WriteLine(JsonCanonicalizer.Canonicalize(json));
Console.WriteLine(JsonCanonicalizer.Hash(json));

// A typed value goes through its source-generated metadata: no reflection, NativeAOT-safe.
var order = new Order("EUR", 12.50m);
Console.WriteLine(JsonCanonicalizer.Hash(order, ExampleJsonContext.Default.Order));

internal sealed record Order(string Currency, decimal Amount);

[JsonSerializable(typeof(Order))]
internal sealed partial class ExampleJsonContext : JsonSerializerContext;
