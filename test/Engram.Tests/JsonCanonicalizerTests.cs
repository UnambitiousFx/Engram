using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace UnambitiousFx.Engram.Tests;

public record Order(string Currency, decimal Amount, string[] Tags);

[JsonSerializable(typeof(Order))]
public partial class TestJsonContext : JsonSerializerContext;

public class JsonCanonicalizerTests
{
    // RFC 8785 §3.2.4, the sample the specification walks through.
    private const string RfcSample =
        "{\"numbers\": [333333333.33333329, 1E30, 4.50, 2e-3, 0.000000000000000000000000001],"
        + " \"string\": \"\\u20ac$\\u000F\\u000aA'\\u0042\\u0022\\u005c\\\\\\\"\\/\","
        + " \"literals\": [null, true, false]}";

    private const string RfcSampleCanonical =
        "{\"literals\":[null,true,false],\"numbers\":[333333333.3333333,1e+30,4.5,0.002,1e-27],"
        + "\"string\":\"€$\\u000f\\nA'B\\\"\\\\\\\\\\\"/\"}";

    [Fact]
    public void Canonicalize_WithRfcSample_ReturnsTheCanonicalFormOfSection324()
    {
        // Arrange (Given)
        using var document = JsonDocument.Parse(RfcSample);

        // Act (When)
        var canonical = JsonCanonicalizer.Canonicalize(document.RootElement);

        // Assert (Then)
        Assert.Equal(RfcSampleCanonical, canonical);
    }

    [Fact]
    public void Canonicalize_WithMembersOutsideTheBmp_SortsByUtf16CodeUnitsNotCodePoints()
    {
        // Arrange (Given): U+1F600 is D83D DE00 in UTF-16, before U+FFFF; by code point it would sort after.
        const string json = "{\"\\uffff\":1,\"\\ud83d\\ude00\":2,\"10\":3,\"2\":4}";

        // Act (When)
        var canonical = JsonCanonicalizer.Canonicalize(json);

        // Assert (Then)
        Assert.Equal("{\"10\":3,\"2\":4,\"😀\":2,\"\uffff\":1}", canonical);
    }

    [Fact]
    public void Canonicalize_WithEscapedCharacters_EscapesOnlyWhatRfc8785Requires()
    {
        // Arrange (Given)
        const string json = "[\"\\u0001\\b\\t\\n\\f\\r\\\"\\\\\\/\\u007f\\u2028é\"]";

        // Act (When)
        var canonical = JsonCanonicalizer.Canonicalize(json);

        // Assert (Then)
        Assert.Equal("[\"\\u0001\\b\\t\\n\\f\\r\\\"\\\\/\u007f\u2028é\"]", canonical);
    }

    [Fact]
    public void Canonicalize_WithNestedWhitespace_RemovesAllWhitespaceOutsideStrings()
    {
        // Arrange (Given)
        const string json = "{ \"b\" : [ 1 , { \"d\" : \"x y\" , \"c\" : true } ] , \"a\" : null }";

        // Act (When)
        var canonical = JsonCanonicalizer.Canonicalize(json);

        // Assert (Then)
        Assert.Equal("{\"a\":null,\"b\":[1,{\"c\":true,\"d\":\"x y\"}]}", canonical);
    }

    [Theory]
    [InlineData("{\"a\":1,\"a\":2}")]
    [InlineData("[1e400]")]
    [InlineData("[\"\\ud800\"]")]
    [InlineData("{\"\\ud800\":1}")]
    public void Canonicalize_WithValueThatHasNoCanonicalForm_ThrowsFormatException(string json)
    {
        // Arrange (Given): a duplicate member, a number beyond the double range, a lone surrogate in a value or a name.

        // Act & Assert (When / Then)
        Assert.Throws<FormatException>(() => JsonCanonicalizer.Canonicalize(json));
    }

    [Fact]
    public void Canonicalize_WithInvalidJsonText_ThrowsJsonException()
    {
        // Arrange (Given)
        const string json = "{\"a\":";

        // Act & Assert (When / Then)
        Assert.ThrowsAny<JsonException>(() => JsonCanonicalizer.Canonicalize(json));
    }

    [Fact]
    public void Canonicalize_WithJsonText_ParsesItInsteadOfTreatingItAsAStringLiteral()
    {
        // Arrange (Given): a string must not bind to the JsonNode overload through the implicit conversion.
        const string json = "{\"a\":1}";

        // Act (When)
        var canonical = JsonCanonicalizer.Canonicalize(json);

        // Assert (Then)
        Assert.Equal("{\"a\":1}", canonical);
    }

    [Fact]
    public void Canonicalize_WithEveryInputKind_GivesTheSameCanonicalForm()
    {
        // Arrange (Given)
        using var document = JsonDocument.Parse(RfcSample);
        var utf8 = Encoding.UTF8.GetBytes(RfcSample);
        var node = JsonNode.Parse(RfcSample);

        // Act (When)
        var fromElement = JsonCanonicalizer.Canonicalize(document.RootElement);
        var fromText = JsonCanonicalizer.Canonicalize(RfcSample);
        var fromUtf8 = JsonCanonicalizer.Canonicalize(utf8.AsSpan());
        var fromNode = JsonCanonicalizer.Canonicalize(node);

        // Assert (Then)
        Assert.Equal(RfcSampleCanonical, fromElement);
        Assert.Equal(RfcSampleCanonical, fromText);
        Assert.Equal(RfcSampleCanonical, fromUtf8);
        Assert.Equal(RfcSampleCanonical, fromNode);
    }

    [Fact]
    public void Canonicalize_WithStringValueNode_ReturnsAJsonString()
    {
        // Arrange (Given): the documented way to canonicalize a JSON string value rather than a JSON text.
        var node = JsonValue.Create("text");

        // Act (When)
        var canonical = JsonCanonicalizer.Canonicalize(node);

        // Assert (Then)
        Assert.Equal("\"text\"", canonical);
    }

    [Fact]
    public void Canonicalize_WithNullNode_ReturnsJsonNull()
    {
        // Arrange (Given)
        JsonNode? node = null;

        // Act (When)
        var canonical = JsonCanonicalizer.Canonicalize(node);

        // Assert (Then)
        Assert.Equal("null", canonical);
    }

    [Fact]
    public void Canonicalize_WithTypedValue_SerializesThroughItsTypeInfoThenCanonicalizes()
    {
        // Arrange (Given)
        var order = new Order("EUR", 12.50m, ["b", "a"]);

        // Act (When)
        var canonical = JsonCanonicalizer.Canonicalize(order, TestJsonContext.Default.Order);

        // Assert (Then)
        Assert.Equal("{\"Amount\":12.5,\"Currency\":\"EUR\",\"Tags\":[\"b\",\"a\"]}", canonical);
    }

    [Fact]
    public void Hash_WithEquivalentTexts_GivesTheSameFingerprint()
    {
        // Arrange (Given)
        const string written = "{ \"amount\" : 12.990, \"currency\": \"EUR\" }";
        const string canonical = "{\"amount\":12.99,\"currency\":\"EUR\"}";

        // Act (When)
        var writtenHash = JsonCanonicalizer.Hash(written);
        var canonicalHash = JsonCanonicalizer.Hash(canonical);

        // Assert (Then)
        Assert.Equal(canonicalHash, writtenHash);
        Assert.Matches("^[0-9a-f]{64}$", writtenHash);
    }

    [Fact]
    public void Hash_WithAnyInputKind_IsTheSha256OfTheUtf8CanonicalForm()
    {
        // Arrange (Given)
        const string json = "{\"b\":2,\"a\":\"é\"}";
        var expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("{\"a\":\"é\",\"b\":2}")))
            .ToLowerInvariant();
        using var document = JsonDocument.Parse(json);

        // Act (When)
        var hashes = new[]
        {
            JsonCanonicalizer.Hash(document.RootElement),
            JsonCanonicalizer.Hash(json),
            JsonCanonicalizer.Hash(Encoding.UTF8.GetBytes(json).AsSpan()),
            JsonCanonicalizer.Hash(JsonNode.Parse(json))
        };

        // Assert (Then)
        Assert.All(hashes, hash => Assert.Equal(expected, hash));
    }

    [Fact]
    public void Hash_WithTypedValue_MatchesTheHashOfItsJsonText()
    {
        // Arrange (Given)
        var order = new Order("EUR", 12.50m, ["b", "a"]);

        // Act (When)
        var typed = JsonCanonicalizer.Hash(order, TestJsonContext.Default.Order);
        var text = JsonCanonicalizer.Hash("{\"Tags\":[\"b\",\"a\"],\"Currency\":\"EUR\",\"Amount\":12.50}");

        // Assert (Then)
        Assert.Equal(text, typed);
    }

    [Fact]
    public void CanonicalizeToUtf8_WithNonAsciiText_ReturnsUtf8WithoutByteOrderMark()
    {
        // Arrange (Given)
        const string json = "[\"€\"]";

        // Act (When)
        var utf8 = JsonCanonicalizer.CanonicalizeToUtf8(json);

        // Assert (Then)
        Assert.Equal(Encoding.UTF8.GetBytes("[\"€\"]"), utf8);
    }

    [Theory]
    [InlineData(0.0, "0")] // negative zero is Appendix B's 8000000000000000
    [InlineData(1.0, "1")]
    [InlineData(-1.5, "-1.5")]
    [InlineData(1e21, "1e+21")]
    [InlineData(1e20, "100000000000000000000")]
    [InlineData(0.000001, "0.000001")]
    [InlineData(0.0000001, "1e-7")]
    [InlineData(123456789012345680000.0, "123456789012345680000")]
    public void FormatNumber_WithBoundaryValues_UsesEcmaScriptLayout(double value, string expected)
    {
        // Act (When)
        var text = JsonCanonicalizer.FormatNumber(value);

        // Assert (Then)
        Assert.Equal(expected, text);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void FormatNumber_WithNonFiniteValue_ThrowsFormatException(double value)
    {
        // Act & Assert (When / Then)
        Assert.Throws<FormatException>(() => JsonCanonicalizer.FormatNumber(value));
    }
}
