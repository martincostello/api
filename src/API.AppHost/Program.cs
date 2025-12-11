// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

var builder = DistributedApplication.CreateBuilder(args);

const string Project = "API";

builder.AddProject<Projects.API>(Project)
       .WithHttpHealthCheck("/version");

var app = builder.Build();

app.Run();
