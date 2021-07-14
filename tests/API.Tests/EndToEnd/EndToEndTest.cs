// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Xunit;

namespace MartinCostello.Api.EndToEnd
{
    [Collection(ApiCollection.Name)]
    [Trait("Category", "EndToEnd")]
    public abstract class EndToEndTest
    {
        protected EndToEndTest(ApiFixture fixture)
        {
            Fixture = fixture;
        }

        protected ApiFixture Fixture { get; }
    }
}
