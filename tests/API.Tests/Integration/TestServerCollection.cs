// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Integration;

/// <summary>
/// A class representing the collection fixture for a test server. This class cannot be inherited.
/// </summary>
[CollectionDefinition(Name)]
public sealed class TestServerCollection : ICollectionFixture<TestServerFixture>
{
    /// <summary>
    /// The name of the test fixture.
    /// </summary>
    public const string Name = "Test server collection";
}
