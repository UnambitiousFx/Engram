# UnambitiousFx.Engram — Guidelines

RFC 8785 JSON Canonicalization Scheme (JCS) for .NET: a deterministic canonical form for any JSON value, and a SHA-256
fingerprint of that form. Namespace: `UnambitiousFx.Engram`. Solution: `Engram.slnx`.

## Layout

| Path | Purpose |
|------|---------|
| `src/Engram` | The library — `JsonCanonicalizer` (`Canonicalize`, `CanonicalizeToUtf8`, `Hash`, `FormatNumber`) |
| `test/Engram.Tests` | Tests, with the RFC 8785 vectors under `vectors/` (see `vectors/SOURCE.md`) |
| `benchmarks/EngramBenchmark` | BenchmarkDotNet benchmarks |
| `examples/BasicUsage` | Console sample; CI publishes it with NativeAOT and checks its fingerprints |
| `docs/` | Docusaurus site (`engram.unambitiousfx.com`) |

## Principles

- **Byte-exact**: the output must match the RFC 8785 reference implementation byte for byte. Any change to
  canonicalization runs against `vectors/official` and `vectors/numbers.txt`; never edit those files to make a test pass.
- **Strict**: input with no canonical form throws `FormatException` (duplicate member names, numbers outside the double
  range, lone surrogates). Never silently produce a form another implementation would not reproduce.
- **Dependency-free and NativeAOT-safe**: BCL and `System.Text.Json` only; typed values go through `JsonTypeInfo<T>`,
  never reflection-based serializer overloads.
- **Modern C#**: file-scoped namespaces, pattern matching, records, collection expressions.

## Conventions

- File-scoped namespaces (`namespace UnambitiousFx.Engram;`).
- Naming: `PascalCase` (public/types/methods), `camelCase` (params/locals), `_camelCase` (private fields), `IPascalCase` (interfaces).
- Always use braces, even single statements.
- XML docs (`<summary>`/`<param>`/`<returns>`/`<exception>`) on all public APIs.
- Multi-target `net8.0;net9.0;net10.0` (latest net10.0); `LangVersion=latest`, `Nullable` + `ImplicitUsings` enabled — set in `build.props`.
  Guard APIs newer than net8 with `#if NET9_0_OR_GREATER` (e.g. `Convert.ToHexStringLower`).
- Central Package Management: add/bump versions in `Directory.Packages.props`, not in `.csproj`.

## Testing

Target 80%+ coverage.

- Name tests `MethodName_Scenario_ExpectedBehavior`.
- AAA = Gherkin (Given-When-Then); comment the sections.
- `[Theory]` + `[InlineData]` / `[MemberData]` for multi-scenario logic.
- Every input overload (`JsonElement`, `string`, UTF-8 span, `JsonNode`, typed) must give the same canonical form.

## Performance

For hot-path changes, add/update benchmarks in `benchmarks/EngramBenchmark` (BenchmarkDotNet).

## Build & Test

```bash
dotnet build Engram.slnx
dotnet test --solution Engram.slnx
```

## Commits & Branches

semantic-release derives the next version from commit messages (see `CONTRIBUTING.md`), so these rules decide what gets released.

- Commits and PR titles follow [Conventional Commits](https://www.conventionalcommits.org/): `type(scope)?: summary`, imperative, lowercase, no trailing period.
- `feat` → minor, `fix`/`perf` → patch. `docs`, `test`, `refactor`, `build`, `ci`, `chore`, `style`, `revert` → no release.
- Breaking change: `type!:` or a `BREAKING CHANGE:` footer → major. Required for any public API break.
- PRs are squash-merged with the PR title as the commit message: the PR title is what counts (checked by `pr-title.yml`).
- Branches: `<type>/<issue>-<slug>`, or `<type>/<slug>` without an issue; kebab-case, `type` from the list above.
- While the repository is private, `release.yml` and the Pages deployment are skipped; they start on their own once it is public.

## Labels

Always label a PR or issue when opening it (`gh pr create --label`, `gh issue create --label`). Use existing labels only (`gh label list`), never create new ones, and add every label that applies.

| Change | Label |
|---|---|
| `feat` / feature request | `enhancement` |
| `fix` / bug report | `bug` |
| `docs` | `documentation` |
| other (`ci`, `chore`, dependencies…) | the closest existing label |
