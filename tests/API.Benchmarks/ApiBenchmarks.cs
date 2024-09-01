// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace MartinCostello.Api.Benchmarks;

[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[MemoryDiagnoser]
public class ApiBenchmarks : IAsyncDisposable
{
    private ApiServer? _app = new();
    private HttpClient? _client;
    private bool _disposed;

    [GlobalSetup]
    public async Task StartServer()
    {
        if (_app is { } app)
        {
            await app.StartAsync();
            _client = app.CreateHttpClient();
        }
    }

    [GlobalCleanup]
    public async Task StopServer()
    {
        if (_app is { } app)
        {
            await app.StopAsync();
            _app = null;
        }
    }

    [Benchmark]
    public async Task<byte[]> Root()
        => await _client!.GetByteArrayAsync("/");

    [Benchmark]
    public async Task<byte[]> Version()
        => await _client!.GetByteArrayAsync("/version");

    [Benchmark]
    public async Task<byte[]> Hash()
    {
        var body = new { algorithm = "sha1", Format = "base64", plaintext = "Hello, world!" };

        using var response = await _client!.PostAsJsonAsync("/tools/hash", body);

        response.EnsureSuccessStatusCode();

        return await response!.Content!.ReadAsByteArrayAsync();
    }

    [Benchmark]
    public async Task<byte[]> Time()
        => await _client!.GetByteArrayAsync("/time");

    [Benchmark]
    public async Task<byte[]> OpenApi()
        => await _client!.GetByteArrayAsync("/openapi/api.json");

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (!_disposed)
        {
            _client?.Dispose();
            _client = null;

            if (_app is not null)
            {
                await _app.DisposeAsync();
                _app = null;
            }
        }

        _disposed = true;
    }
}
