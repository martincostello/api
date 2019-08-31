// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Options
{
    /// <summary>
    /// A class representing the documentation options for the site. This class cannot be inherited.
    /// </summary>
    public sealed class DocumentationOptions
    {
        /// <summary>
        /// Gets or sets the relative path to the location of the documentation.
        /// </summary>
        public string Location { get; set; } = string.Empty;
    }
}
