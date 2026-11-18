// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDotnetProject("API", "../API")
       .WithHttpHealthCheck("/version");

var app = builder.Build();

app.Run();
