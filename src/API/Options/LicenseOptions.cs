// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Options
{
    /// <summary>
    /// A class representing the license options for the API. This class cannot be inherited.
    /// </summary>
    public sealed class LicenseOptions
    {
        /// <summary>
        /// Gets or sets the license name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the license URL.
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
