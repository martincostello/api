// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Api;

var builder = WebApplication.CreateBuilder(args);

ApiBuilder.Configure(builder).Run();
