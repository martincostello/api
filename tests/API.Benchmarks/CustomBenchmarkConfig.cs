// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace MartinCostello.Api.Benchmarks;

public class CustomBenchmarkConfig : ManualConfig
{
    public CustomBenchmarkConfig()
        : base()
    {
        var job = Job.Default
            .WithId("API")
            .WithArguments([new MsBuildArgument("/p:UseArtifactsOutput=false")]);

        AddJob(job);
        AddDiagnoser(MemoryDiagnoser.Default);
        AddDiagnoser(new EventPipeProfiler(EventPipeProfile.CpuSampling));
    }
}
