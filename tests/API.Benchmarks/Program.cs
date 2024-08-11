// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using MartinCostello.Api.Benchmarks;

if (args.SequenceEqual(["--test"]))
{
    await using var benchmark = new ApiBenchmarks();
    await benchmark.StartServer();

    _ = await benchmark.Hash();
    _ = await benchmark.Time();

    await benchmark.StopServer();
}
else
{
    BenchmarkRunner.Run<ApiBenchmarks>(args: args);
}
