# Contributing to UnambitiousFx.Engram

Thank you for considering contributing to UnambitiousFx.Engram!

## Code of Conduct

We expect all contributors to be respectful and considerate, to accept constructive criticism gracefully, and to focus
on what is best for the community.

## How Can I Contribute?

### Reporting Bugs

Check existing issues first, then use the **bug report template**. For a canonicalization bug, include the JSON input,
the output you got and the output you expected (ideally from another RFC 8785 implementation).

### Suggesting Enhancements

Use the **feature request template**: the use case, the proposed API, and the alternatives you considered.

### Pull Requests

1. Create your branch from `main` (`<type>/<issue>-<slug>`, e.g. `feat/12-ibufferwriter-output`).
2. Follow the coding standards below.
3. Add tests; canonicalization changes must keep every RFC 8785 vector passing.
4. Update the XML docs, the README and `docs/` if the public API changes.
5. Run `dotnet test --solution Engram.slnx`.
6. Run the benchmarks if the change touches the hot path.

PR titles follow [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `perf:`, `docs:`,
`test:`, `refactor:`, `build:`, `ci:`, `chore:`): PRs are squash-merged and the title becomes the commit that decides the
release (see [Release Process](#release-process)).

## Development Environment Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download), plus the .NET 8 and 9 runtimes to run the tests on every target
- An IDE: [JetBrains Rider](https://www.jetbrains.com/rider/), [Visual Studio](https://visualstudio.microsoft.com/), or
  [VS Code](https://code.visualstudio.com/)

### Getting Started

```bash
git clone https://github.com/UnambitiousFx/Engram.git
cd Engram
dotnet build Engram.slnx
dotnet test --solution Engram.slnx
```

### Project Structure

```
Engram/
├── src/Engram/                  # The library
├── test/Engram.Tests/           # Tests and the RFC 8785 vectors (vectors/SOURCE.md)
├── benchmarks/EngramBenchmark/  # BenchmarkDotNet benchmarks
├── examples/BasicUsage/         # Console sample, published with NativeAOT in CI
└── docs/                        # Docusaurus site
```

## Coding Standards

- File-scoped namespaces (`namespace UnambitiousFx.Engram;`), braces on every block, modern C# syntax.
- `PascalCase` for public members and types, `camelCase` for parameters and locals, `_camelCase` for private fields.
- XML documentation on every public API, with `<exception>` for every exception it throws.
- No runtime dependency beyond the BCL and `System.Text.Json`; nothing that breaks trimming or NativeAOT.

## Testing Your Changes

```bash
# All tests, on net8.0, net9.0 and net10.0
dotnet test --solution Engram.slnx

# With coverage (Microsoft Testing Platform)
dotnet test --solution Engram.slnx --coverage --coverage-output-format cobertura --coverage-settings ./test/coverage.settings.xml

# The NativeAOT sample
dotnet publish examples/BasicUsage -c Release -r <rid> --self-contained
```

The vectors under `test/Engram.Tests/vectors` are copied unmodified from the RFC and its reference implementation; never
edit them to make a test pass.

### Running Benchmarks

```bash
dotnet run --project benchmarks/EngramBenchmark -c Release
```

## Release Process

(For maintainers)

Releases are automatic. Every push to `main` that passes CI runs [semantic-release](https://semantic-release.gitbook.io/)
(`.github/workflows/release.yml`):

- `feat:` → minor, `fix:`/`perf:` → patch, `!` or a `BREAKING CHANGE:` footer → major; other types don't release.
- The packages are pushed to NuGet first, then the `vX.Y.Z` tag and the GitHub release are created.

While the repository is private, the release job and the documentation deployment are skipped.

## License

By contributing to UnambitiousFx.Engram, you agree that your contributions will be licensed under the [MIT License](LICENSE).
