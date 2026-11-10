using System.Globalization;
using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using UnambitiousFx.Engram;

namespace UnambitiousFx.Benchmarks.EngramBenchmark;

[MemoryDiagnoser]
public class CanonicalizeBenchmark
{
    private JsonDocument _document = null!;
    private byte[] _utf8 = [];

    [Params(10, 1_000)]
    public int Items { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var json = new StringBuilder("{\"items\":[");
        for (var i = 0; i < Items; i++)
        {
            if (i > 0)
            {
                json.Append(',');
            }

            json.Append("{\"name\":\"item ").Append(i).Append("\",\"price\":").Append((i * 1.25).ToString(CultureInfo.InvariantCulture))
                .Append(",\"tags\":[\"a\",\"b\"],\"active\":true,\"id\":").Append(i).Append('}');
        }

        json.Append("],\"currency\":\"EUR\"}");
        _utf8 = Encoding.UTF8.GetBytes(json.ToString());
        _document = JsonDocument.Parse(_utf8);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _document.Dispose();
    }

    [Benchmark(Baseline = true)]
    public string Canonicalize()
    {
        return JsonCanonicalizer.Canonicalize(_document.RootElement);
    }

    [Benchmark]
    public string Hash()
    {
        return JsonCanonicalizer.Hash(_document.RootElement);
    }

    [Benchmark]
    public string HashFromUtf8()
    {
        return JsonCanonicalizer.Hash(_utf8.AsSpan());
    }
}
