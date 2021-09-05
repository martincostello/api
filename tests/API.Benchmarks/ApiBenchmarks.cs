// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace MartinCostello.Api.Benchmarks;

[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[MemoryDiagnoser]
public class ApiBenchmarks : IDisposable
{
    private const string ServerUrl = "https://localhost:5001"; // TODO Find dynamically by querying the host

    private readonly HttpClient _client;
    private bool _disposed;
    private WebApplication? _app;

    public ApiBenchmarks()
    {
        // TODO Improve the code to find the path for the content root
        var builder = WebApplication.CreateBuilder(new[] { "--contentRoot=" + Path.Join("..", "..", "..", "..", "..", "src", "API") });

        builder.Logging.ClearProviders();

        _app = ApiBuilder.Configure(builder);

        _client = new HttpClient()
        {
            BaseAddress = new Uri(ServerUrl, UriKind.Absolute),
        };
    }

    ~ApiBenchmarks()
    {
        Dispose(false);
    }

    [GlobalSetup]
    public async Task StartServer()
    {
        if (_app != null)
        {
            await _app.StartAsync();
        }
    }

    [GlobalCleanup]
    public async Task StopServer()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            _app = null;
        }
    }

    [Benchmark]
    public async Task<byte[]> Hash()
    {
        var body = new { algorithm = "sha1", Format = "base64", plaintext = "Hello, world!" };

        using var response = await _client.PostAsJsonAsync("/tools/hash", body);
        return await response!.Content!.ReadAsByteArrayAsync();
    }

    [Benchmark]
    public async Task<byte[]> Time()
        => await _client.GetByteArrayAsync("/time");

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _client?.Dispose();
        }

        _disposed = true;
    }
}
