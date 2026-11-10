using System.Globalization;
using System.Text.Json;

namespace UnambitiousFx.Engram.Tests;

public class Rfc8785VectorsTests
{
    private static string Vector(params string[] parts)
    {
        return Path.Combine([AppContext.BaseDirectory, "vectors", .. parts]);
    }

    public static TheoryData<string> OfficialCases()
    {
        var data = new TheoryData<string>();
        foreach (var file in Directory.EnumerateFiles(Vector("official", "input"), "*.json").Order(StringComparer.Ordinal))
        {
            data.Add(Path.GetFileName(file));
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(OfficialCases))]
    public void CanonicalizeToUtf8_WithReferenceInput_ReproducesReferenceOutputByteForByte(string name)
    {
        // Arrange (Given)
        using var input = JsonDocument.Parse(File.ReadAllBytes(Vector("official", "input", name)));
        var expected = File.ReadAllBytes(Vector("official", "output", name));

        // Act (When)
        var canonical = JsonCanonicalizer.CanonicalizeToUtf8(input.RootElement);

        // Assert (Then)
        Assert.Equal(expected, canonical);
    }

    public static TheoryData<string, string> AppendixB()
    {
        var data = new TheoryData<string, string>();
        foreach (var line in File.ReadAllLines(Vector("numbers.txt")).Where(l => l.Length > 0))
        {
            var parts = line.Split(',');
            data.Add(parts[0], parts[1]);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AppendixB))]
    public void FormatNumber_WithAppendixBSample_MatchesExpectedText(string ieeeHex, string expected)
    {
        // Arrange (Given)
        var bits = ulong.Parse(ieeeHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var value = BitConverter.Int64BitsToDouble(unchecked((long)bits));

        // Act & Assert (When / Then)
        if (expected == "error")
        {
            Assert.Throws<FormatException>(() => JsonCanonicalizer.FormatNumber(value));
        }
        else
        {
            Assert.Equal(expected, JsonCanonicalizer.FormatNumber(value));
        }
    }
}
