// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Benchmarks
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using BenchmarkDotNet.Attributes;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    [MemoryDiagnoser]
    public class TimeBenchmarks : IDisposable
    {
        private readonly IWebHost _host;
        private readonly HttpClient _client;
        private readonly Uri _uri;
        private bool _disposed;

        public TimeBenchmarks()
        {
            _host = WebHost.CreateDefaultBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5002")
                .ConfigureLogging((builder) => builder.ClearProviders().SetMinimumLevel(LogLevel.Error))
                .Build();

            _client = new HttpClient();
            _uri = new Uri("http://localhost:5002/time", UriKind.Absolute);
        }

        ~TimeBenchmarks()
        {
            Dispose(false);
        }

        [GlobalSetup]
        public async Task StartServer()
        {
            await _host.StartAsync();
        }

        [Benchmark]
        public async Task<byte[]> Time()
        {
            return await _client.GetByteArrayAsync(_uri);
        }

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
