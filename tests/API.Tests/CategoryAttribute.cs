// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Xunit.v3;

namespace MartinCostello.Api;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class CategoryAttribute(string category) : Attribute, ITraitAttribute
{
    public string Category { get; } = category;

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => [new("Category", Category)];
}
