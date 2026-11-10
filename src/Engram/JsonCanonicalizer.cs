using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace UnambitiousFx.Engram;

/// <summary>
///     The JSON Canonicalization Scheme of RFC 8785: members sorted by UTF-16 code units, minimal escaping, no
///     whitespace, numbers serialized as ECMAScript does. Two JSON texts that carry the same data give the same
///     canonical form, so its bytes can be compared, hashed or signed.
/// </summary>
/// <remarks>
///     Every refusal (a duplicate member name, a number outside the IEEE 754 double range, a lone surrogate) throws a
///     <see cref="FormatException" />: RFC 8785 has no canonical form for such input.
/// </remarks>
public static class JsonCanonicalizer
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    /// <summary>Returns the RFC 8785 canonical form of a JSON value.</summary>
    /// <param name="element">The JSON value to canonicalize.</param>
    /// <returns>The canonical JSON text.</returns>
    /// <exception cref="FormatException">The value has no RFC 8785 canonical form.</exception>
    public static string Canonicalize(JsonElement element)
    {
        var output = new StringBuilder();
        Write(element, output);
        return output.ToString();
    }

    /// <summary>Returns the RFC 8785 canonical form of a UTF-8 JSON text.</summary>
    /// <param name="utf8Json">The JSON text, UTF-8 encoded.</param>
    /// <returns>The canonical JSON text.</returns>
    /// <exception cref="JsonException">The text is not valid JSON.</exception>
    /// <exception cref="FormatException">The value has no RFC 8785 canonical form.</exception>
    public static string Canonicalize(ReadOnlySpan<byte> utf8Json)
    {
        var reader = new Utf8JsonReader(utf8Json);
        using var document = JsonDocument.ParseValue(ref reader);
        return Canonicalize(document.RootElement);
    }

    /// <summary>Parses a JSON text and returns its RFC 8785 canonical form.</summary>
    /// <param name="json">The JSON text.</param>
    /// <returns>The canonical JSON text.</returns>
    /// <exception cref="JsonException">The text is not valid JSON.</exception>
    /// <exception cref="FormatException">The value has no RFC 8785 canonical form.</exception>
    public static string Canonicalize(string json)
    {
        // Without this overload a string would bind to Canonicalize(JsonNode?) through JsonNode's implicit
        // conversion and be canonicalized as a JSON string literal instead of being parsed.
        ArgumentNullException.ThrowIfNull(json);
        using var document = JsonDocument.Parse(json);
        return Canonicalize(document.RootElement);
    }

    /// <summary>Returns the RFC 8785 canonical form of a JSON node; a <see langword="null" /> node is JSON <c>null</c>.</summary>
    /// <param name="node">The JSON node to canonicalize.</param>
    /// <returns>The canonical JSON text.</returns>
    /// <exception cref="FormatException">The value has no RFC 8785 canonical form.</exception>
    public static string Canonicalize(JsonNode? node)
    {
        return Canonicalize(Utf8Of(node).WrittenSpan);
    }

    /// <summary>Serializes a value with its source-generated metadata, then returns its RFC 8785 canonical form.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <param name="typeInfo">The serialization metadata of <typeparamref name="T" />; NativeAOT-safe.</param>
    /// <returns>The canonical JSON text.</returns>
    /// <exception cref="FormatException">The value has no RFC 8785 canonical form.</exception>
    public static string Canonicalize<T>(T value, JsonTypeInfo<T> typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        return Canonicalize(JsonSerializer.SerializeToUtf8Bytes(value, typeInfo));
    }

    /// <summary>Returns the RFC 8785 canonical form of a JSON value, UTF-8 encoded: the bytes to hash or sign.</summary>
    /// <param name="element">The JSON value to canonicalize.</param>
    /// <returns>The canonical JSON text as UTF-8 bytes, without a byte order mark.</returns>
    /// <exception cref="FormatException">The value has no RFC 8785 canonical form.</exception>
    public static byte[] CanonicalizeToUtf8(JsonElement element)
    {
        return ToUtf8(Canonicalize(element));
    }

    /// <inheritdoc cref="CanonicalizeToUtf8(JsonElement)" />
    /// <param name="utf8Json">The JSON text, UTF-8 encoded.</param>
    /// <exception cref="JsonException">The text is not valid JSON.</exception>
    public static byte[] CanonicalizeToUtf8(ReadOnlySpan<byte> utf8Json)
    {
        return ToUtf8(Canonicalize(utf8Json));
    }

    /// <inheritdoc cref="CanonicalizeToUtf8(JsonElement)" />
    /// <param name="json">The JSON text.</param>
    /// <exception cref="JsonException">The text is not valid JSON.</exception>
    public static byte[] CanonicalizeToUtf8(string json)
    {
        return ToUtf8(Canonicalize(json));
    }

    /// <inheritdoc cref="CanonicalizeToUtf8(JsonElement)" />
    /// <param name="node">The JSON node to canonicalize; <see langword="null" /> is JSON <c>null</c>.</param>
    public static byte[] CanonicalizeToUtf8(JsonNode? node)
    {
        return ToUtf8(Canonicalize(node));
    }

    /// <inheritdoc cref="CanonicalizeToUtf8(JsonElement)" />
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <param name="typeInfo">The serialization metadata of <typeparamref name="T" />; NativeAOT-safe.</param>
    public static byte[] CanonicalizeToUtf8<T>(T value, JsonTypeInfo<T> typeInfo)
    {
        return ToUtf8(Canonicalize(value, typeInfo));
    }

    /// <summary>Returns the content fingerprint of a JSON value: the lowercase hex SHA-256 of its UTF-8 canonical form.</summary>
    /// <param name="element">The JSON value to fingerprint.</param>
    /// <returns>64 lowercase hexadecimal characters.</returns>
    /// <exception cref="FormatException">The value has no RFC 8785 canonical form.</exception>
    public static string Hash(JsonElement element)
    {
        return Hex(CanonicalizeToUtf8(element));
    }

    /// <inheritdoc cref="Hash(JsonElement)" />
    /// <param name="utf8Json">The JSON text, UTF-8 encoded.</param>
    /// <exception cref="JsonException">The text is not valid JSON.</exception>
    public static string Hash(ReadOnlySpan<byte> utf8Json)
    {
        return Hex(CanonicalizeToUtf8(utf8Json));
    }

    /// <inheritdoc cref="Hash(JsonElement)" />
    /// <param name="json">The JSON text.</param>
    /// <exception cref="JsonException">The text is not valid JSON.</exception>
    public static string Hash(string json)
    {
        return Hex(CanonicalizeToUtf8(json));
    }

    /// <inheritdoc cref="Hash(JsonElement)" />
    /// <param name="node">The JSON node to fingerprint; <see langword="null" /> is JSON <c>null</c>.</param>
    public static string Hash(JsonNode? node)
    {
        return Hex(CanonicalizeToUtf8(node));
    }

    /// <inheritdoc cref="Hash(JsonElement)" />
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <param name="typeInfo">The serialization metadata of <typeparamref name="T" />; NativeAOT-safe.</param>
    public static string Hash<T>(T value, JsonTypeInfo<T> typeInfo)
    {
        return Hex(CanonicalizeToUtf8(value, typeInfo));
    }

    /// <summary>
    ///     Formats a number as ECMAScript <c>Number.prototype.toString</c> does (ECMA-262 §7.1.12.1), as RFC 8785
    ///     §3.2.2.3 requires: the shortest digits that round-trip, exponent notation from 1e21 and below 1e-6.
    /// </summary>
    /// <param name="value">The number to format.</param>
    /// <returns>The number as it appears in a canonical JSON text; negative zero gives <c>0</c>.</returns>
    /// <exception cref="FormatException"><paramref name="value" /> is NaN or an infinity, which JSON cannot represent.</exception>
    public static string FormatNumber(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new FormatException("NaN and infinities have no JSON representation.");
        }

        if (value == 0)
        {
            return "0"; // negative zero too
        }

        var (digits, n) = ShortestDigits(Math.Abs(value));
        var k = digits.Length;

        string body;
        if (k <= n && n <= 21)
        {
            body = digits + new string('0', n - k);
        }
        else if (0 < n && n <= 21)
        {
            body = digits[..n] + "." + digits[n..];
        }
        else if (-6 < n && n <= 0)
        {
            body = "0." + new string('0', -n) + digits;
        }
        else
        {
            var exponent = n - 1;
            var sign = exponent < 0 ? "-" : "+";
            var mantissa = k == 1 ? digits : digits[..1] + "." + digits[1..];
            body = mantissa + "e" + sign + Math.Abs(exponent).ToString(CultureInfo.InvariantCulture);
        }

        return value < 0 ? "-" + body : body;
    }

    /// <summary>
    ///     The shortest digit string that round-trips, and <c>n</c> such that value = 0.digits × 10^n. .NET's "R"
    ///     format already yields the shortest round-trip digits; only its layout differs from ECMAScript's.
    /// </summary>
    private static (string Digits, int N) ShortestDigits(double positive)
    {
        var text = positive.ToString("R", CultureInfo.InvariantCulture); // "1E+23", "0.0001", "123.45", "5E-324"
        var exponent = 0;
        var e = text.IndexOfAny(['E', 'e']);
        if (e >= 0)
        {
            exponent = int.Parse(text[(e + 1)..], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            text = text[..e];
        }

        var point = text.IndexOf('.');
        var integerPart = point >= 0 ? text[..point] : text;
        var fraction = point >= 0 ? text[(point + 1)..] : "";

        var digits = integerPart + fraction;
        var n = integerPart.Length + exponent;

        var leading = 0;
        while (leading < digits.Length - 1 && digits[leading] == '0')
        {
            leading++;
        }

        return (digits[leading..].TrimEnd('0'), n - leading);
    }

    private static ArrayBufferWriter<byte> Utf8Of(JsonNode? node)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        if (node is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            node.WriteTo(writer);
        }

        writer.Flush();
        return buffer;
    }

    private static byte[] ToUtf8(string canonical)
    {
        try
        {
            return StrictUtf8.GetBytes(canonical);
        }
        catch (EncoderFallbackException invalid)
        {
            throw new FormatException("A string holds a lone surrogate, which RFC 8785 cannot represent.", invalid);
        }
    }

    private static string Hex(byte[] utf8)
    {
        var hash = SHA256.HashData(utf8);
#if NET9_0_OR_GREATER
        return Convert.ToHexStringLower(hash);
#else
        return Convert.ToHexString(hash).ToLowerInvariant();
#endif
    }

    private static void Write(JsonElement element, StringBuilder output)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                WriteObject(element, output);
                break;

            case JsonValueKind.Array:
                output.Append('[');
                var first = true;
                foreach (var item in element.EnumerateArray())
                {
                    if (!first)
                    {
                        output.Append(',');
                    }

                    first = false;
                    Write(item, output);
                }

                output.Append(']');
                break;

            case JsonValueKind.String:
                WriteString(ReadStringOrThrow(element), output);
                break;

            case JsonValueKind.Number:
                output.Append(FormatNumber(double.Parse(element.GetRawText(), NumberStyles.Float, CultureInfo.InvariantCulture)));
                break;

            case JsonValueKind.True:
                output.Append("true");
                break;

            case JsonValueKind.False:
                output.Append("false");
                break;

            case JsonValueKind.Null:
                output.Append("null");
                break;

            default:
                throw new FormatException($"Unexpected JSON value kind {element.ValueKind}.");
        }
    }

    private static void WriteObject(JsonElement element, StringBuilder output)
    {
        var members = new List<(string Name, JsonElement Value)>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            var name = ReadNameOrThrow(property);
            if (!seen.Add(name))
            {
                throw new FormatException($"Member '{name}' appears twice: RFC 8785 requires unique names.");
            }

            members.Add((name, property.Value));
        }

        members.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name)); // UTF-16 code units
        output.Append('{');
        for (var i = 0; i < members.Count; i++)
        {
            if (i > 0)
            {
                output.Append(',');
            }

            WriteString(members[i].Name, output);
            output.Append(':');
            Write(members[i].Value, output);
        }

        output.Append('}');
    }

    // A \uD800-\uDFFF escape with no matching pair makes GetString and Name throw InvalidOperationException while
    // unescaping; RFC 8785 cannot represent a lone surrogate, so it becomes the FormatException of every other refusal.
    private static string ReadStringOrThrow(JsonElement element)
    {
        try
        {
            return element.GetString()!;
        }
        catch (InvalidOperationException invalid)
        {
            throw LoneSurrogate(invalid);
        }
    }

    private static string ReadNameOrThrow(JsonProperty property)
    {
        try
        {
            return property.Name;
        }
        catch (InvalidOperationException invalid)
        {
            throw LoneSurrogate(invalid);
        }
    }

    private static FormatException LoneSurrogate(Exception inner)
    {
        return new FormatException("A string holds a lone surrogate, which RFC 8785 cannot represent.", inner);
    }

    /// <summary>RFC 8785 §3.2.2.2: escape <c>"</c>, <c>\</c> and control characters; nothing else.</summary>
    private static void WriteString(string value, StringBuilder output)
    {
        output.Append('"');
        foreach (var c in value)
        {
            switch (c)
            {
                case '"':
                    output.Append("\\\"");
                    break;
                case '\\':
                    output.Append("\\\\");
                    break;
                case '\b':
                    output.Append("\\b");
                    break;
                case '\f':
                    output.Append("\\f");
                    break;
                case '\n':
                    output.Append("\\n");
                    break;
                case '\r':
                    output.Append("\\r");
                    break;
                case '\t':
                    output.Append("\\t");
                    break;
                default:
                    if (c < 0x20)
                    {
                        output.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        output.Append(c);
                    }

                    break;
            }
        }

        output.Append('"');
    }
}
