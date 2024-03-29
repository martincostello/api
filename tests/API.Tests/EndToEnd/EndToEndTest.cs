﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.EndToEnd;

[Collection(ApiCollection.Name)]
[Trait("Category", "EndToEnd")]
public abstract class EndToEndTest(ApiFixture fixture)
{
    protected ApiFixture Fixture { get; } = fixture;
}
