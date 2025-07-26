// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using MartinCostello.Api.Benchmarks;

if (args.SequenceEqual(["--test"]))
{
    await using var benchmark = new ApiBenchmarks();
    await benchmark.StartServer();

    try
    {
        _ = await benchmark.Root();
        _ = await benchmark.Version();
        _ = await benchmark.Hash();
        _ = await benchmark.Time();
        _ = await benchmark.OpenApi();
    }
    finally
    {
        await benchmark.StopServer();
    }

    return 0;
}
else
{
    var summary = BenchmarkRunner.Run<ApiBenchmarks>(args: args);
    return summary.Reports.Any((p) => !p.Success) ? 1 : 0;
}
