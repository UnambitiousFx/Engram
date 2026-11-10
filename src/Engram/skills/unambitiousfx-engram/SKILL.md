---
name: unambitiousfx-engram
description: Canonicalize, fingerprint, compare or sign JSON in .NET with UnambitiousFx.Engram (RFC 8785, JCS). Use when a .NET project references UnambitiousFx.Engram, or when code needs a stable hash of JSON, idempotency keys from a payload, deduplication of JSON documents, or bytes to sign.
---

# UnambitiousFx.Engram

RFC 8785 JSON Canonicalization Scheme. Namespace: `UnambitiousFx.Engram`. One static class: `JsonCanonicalizer`.

## Rules

- Never hash or sign the output of `JsonSerializer.Serialize` directly: member order, whitespace and number formatting
  are not stable. Hash `JsonCanonicalizer.Hash(...)`, sign `JsonCanonicalizer.CanonicalizeToUtf8(...)`.
- For typed values use the `JsonTypeInfo<T>` overload with a source-generated `JsonSerializerContext` (NativeAOT-safe).
- Input with no canonical form throws `FormatException` (duplicate member names, a number outside the double range,
  a lone surrogate); invalid JSON text throws `JsonException`. Catch them at the trust boundary.
- A canonical form is about the JSON, not the domain: `12.5` and `12.50` are the same number, but `"12.50"` (a string)
  is different. Normalize domain values before serializing if they must compare equal.

## API

```csharp
using UnambitiousFx.Engram;

string canonical = JsonCanonicalizer.Canonicalize(json);           // string, JsonElement, ReadOnlySpan<byte>, JsonNode?
byte[] toSign    = JsonCanonicalizer.CanonicalizeToUtf8(element);  // UTF-8, no BOM
string hash      = JsonCanonicalizer.Hash(node);                   // lowercase hex SHA-256, 64 chars
string typed     = JsonCanonicalizer.Hash(order, AppJsonContext.Default.Order);
string number    = JsonCanonicalizer.FormatNumber(1e21);           // "1e+21" (ECMAScript layout)
```

`JsonNode? null` canonicalizes to `null`. A `string` argument is parsed as JSON text, not treated as a JSON string value.

## Idempotency key from a request body

```csharp
app.MapPost("/imports", async (HttpRequest request) =>
{
    using var body = await JsonDocument.ParseAsync(request.Body);
    var key = JsonCanonicalizer.Hash(body.RootElement);
    // look up `key` before processing; the same payload, however it is formatted, has the same key
});
```
