# UnambitiousFx.Engram

[![Build Status](https://github.com/UnambitiousFx/Engram/workflows/CI/badge.svg)](https://github.com/UnambitiousFx/Engram/actions)
[![NuGet](https://img.shields.io/nuget/v/UnambitiousFx.Engram.svg)](https://www.nuget.org/packages/UnambitiousFx.Engram/)
[![codecov](https://codecov.io/gh/UnambitiousFx/Engram/branch/main/graph/badge.svg)](https://codecov.io/gh/UnambitiousFx/Engram)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Documentation](https://img.shields.io/badge/docs-engram.unambitiousfx.com-blue)](https://engram.unambitiousfx.com)

[RFC 8785](https://www.rfc-editor.org/rfc/rfc8785) JSON Canonicalization Scheme (JCS) for .NET: one deterministic form
for any JSON value, and a SHA-256 fingerprint of that form.

Two JSON texts can carry the same data and still differ byte for byte: member order, whitespace, number formatting
(`1.0` and `1`, `1e3` and `1000`), string escapes. Engram turns both into the same canonical text, so you can
**compare, hash, sign or deduplicate** JSON.

> An *engram* is the stable trace a memory leaves in the brain. Here, it is the trace a JSON document leaves: its
> canonical form and its fingerprint.

## 🔧 Compatibility & support

- **Dependency-free**: only the BCL and `System.Text.Json`.
- **NativeAOT and trimming compatible**: CI publishes and runs an AOT binary on every change.
- **Supported .NET versions**: `net8.0`, `net9.0`, `net10.0`, and `net11.0`.

## 🎯 Features

- **RFC 8785 canonical form** of a `JsonElement`, a `JsonNode`, a JSON text (string or UTF-8 bytes), or a typed value
  serialized through its source-generated `JsonTypeInfo<T>`.
- **Content fingerprint**: the lowercase hex SHA-256 of the UTF-8 canonical form.
- **ECMAScript number formatting**: the shortest digits that round-trip, laid out as `Number.prototype.toString` does,
  as RFC 8785 requires.
- **Strict**: duplicate member names, numbers outside the double range and lone surrogates throw a `FormatException`
  instead of producing a form another implementation would not reproduce.
- **Tested against the reference vectors** of the RFC (Appendix B) and of its reference implementation.

## 📦 Installation

```bash
dotnet add package UnambitiousFx.Engram
```

## 🚀 Quick start

```csharp
using UnambitiousFx.Engram;

var a = """{ "currency": "EUR", "amount": 12.50 }""";
var b = """{"amount":12.5,"currency":"EUR"}""";

JsonCanonicalizer.Canonicalize(a);   // {"amount":12.5,"currency":"EUR"}
JsonCanonicalizer.Hash(a) == JsonCanonicalizer.Hash(b);   // true

// the bytes to sign or HMAC
byte[] payload = JsonCanonicalizer.CanonicalizeToUtf8(a);

// a typed value, through its source-generated metadata (no reflection)
string fingerprint = JsonCanonicalizer.Hash(order, AppJsonContext.Default.Order);
```

## 💡 Use cases

| Need | How |
|---|---|
| Idempotency, deduplication | Store `Hash(payload)`; a second submission of the same data has the same fingerprint. |
| Change detection | Compare the fingerprint of the new export with the last one, without keeping the old document. |
| Signatures | Sign `CanonicalizeToUtf8(value)`; a verifier that re-serializes the JSON still checks the same bytes. |
| ETags | Use the fingerprint of the response body instead of a timestamp. |

## 📚 API

| Method | Returns |
|---|---|
| `Canonicalize(…)` | The canonical JSON text. |
| `CanonicalizeToUtf8(…)` | The canonical JSON text as UTF-8 bytes, without a byte order mark. |
| `Hash(…)` | The lowercase hex SHA-256 of the UTF-8 canonical form (64 characters). |
| `FormatNumber(double)` | A number as it appears in a canonical text. |

Each of the first three accepts a `JsonElement`, a JSON `string`, UTF-8 bytes (`ReadOnlySpan<byte>`), a `JsonNode?`
(`null` is JSON `null`), or a value with its `JsonTypeInfo<T>`.

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## 📄 License

[MIT](LICENSE). The test vectors under `test/Engram.Tests/vectors/official` come from the
[RFC 8785 reference implementation](https://github.com/cyberphone/json-canonicalization) under the Apache License 2.0.
