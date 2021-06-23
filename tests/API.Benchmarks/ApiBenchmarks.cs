// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MartinCostello.Api.Benchmarks
{
    [EventPipeProfiler(EventPipeProfile.CpuSampling)]
    [MemoryDiagnoser]
    public class ApiBenchmarks : IDisposable
    {
        private const string ServerUrl = "http://localhost:5002";

        private readonly HttpClient _client;
        private bool _disposed;
        private IHost? _host;

        public ApiBenchmarks()
        {
            _host = Api.Program.CreateHostBuilder(Array.Empty<string>())
                .UseEnvironment("Development")
                .ConfigureLogging((builder) => builder.ClearProviders().SetMinimumLevel(LogLevel.Error))
                .ConfigureWebHostDefaults((builder) => builder.UseUrls(ServerUrl))
                .Build();

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
            if (_host != null)
            {
                await _host.StartAsync();
            }
        }

        [GlobalCleanup]
        public async Task StopServer()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
                _host = null;
            }
        }

        [Benchmark]
        public async Task<byte[]> Hash()
        {
            var body = new { algorithm = "sha1", Format = "base64", plaintext = "Hello, world!" };

            using var response = await _client.PostAsJsonAsync("/hash", body);
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
                _host?.Dispose();
            }

            _disposed = true;
        }
    }
}
