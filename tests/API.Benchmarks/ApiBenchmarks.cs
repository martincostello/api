// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace MartinCostello.Api.Benchmarks;

[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[MemoryDiagnoser]
public class ApiBenchmarks : IAsyncDisposable
{
    private WebApplication? _app;
    private HttpClient? _client;
    private bool _disposed;

    public ApiBenchmarks()
    {
        var builder = WebApplication.CreateBuilder(["--contentRoot=" + GetContentRoot()]);

        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("https://127.0.0.1:0");

        _app = ApiBuilder.Configure(builder);
    }

    [GlobalSetup]
    public async Task StartServer()
    {
        if (_app != null)
        {
            await _app.StartAsync();

            var server = _app.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>();

            var baseAddress = addresses!.Addresses
                .Select((p) => new Uri(p))
                .Last();

#pragma warning disable CA2000
#pragma warning disable CA5400
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            };

            _client = new HttpClient(handler, disposeHandler: true)
            {
                BaseAddress = baseAddress,
            };
#pragma warning restore CA2000
#pragma warning restore CA5400
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

        using var response = await _client!.PostAsJsonAsync("/tools/hash", body);

        response.EnsureSuccessStatusCode();

        return await response!.Content!.ReadAsByteArrayAsync();
    }

    [Benchmark]
    public async Task<byte[]> Time()
        => await _client!.GetByteArrayAsync("/time");

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (!_disposed)
        {
            _client?.Dispose();

            if (_app is not null)
            {
                await _app.DisposeAsync();
            }
        }

        _disposed = true;
    }

    private static string GetContentRoot()
    {
        string contentRoot = string.Empty;
        var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(typeof(ApiBenchmarks).Assembly.Location)!);

        do
        {
            string? solutionPath = Directory.EnumerateFiles(directoryInfo.FullName, "API.sln").FirstOrDefault();

            if (solutionPath != null)
            {
                contentRoot = Path.GetFullPath(Path.Combine(directoryInfo.FullName, "src", "API"));
                break;
            }

            directoryInfo = directoryInfo.Parent;
        }
        while (directoryInfo is not null);

        return contentRoot;
    }
}
