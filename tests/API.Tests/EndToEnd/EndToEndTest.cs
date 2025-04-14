// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.EndToEnd;

[Category("EndToEnd")]
[Collection<ApiCollection>]
public abstract class EndToEndTest(ApiFixture fixture)
{
    protected virtual CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    protected ApiFixture Fixture { get; } = fixture;
}
