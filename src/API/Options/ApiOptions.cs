// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Options
{
    /// <summary>
    /// A class representing the API options for the site. This class cannot be inherited.
    /// </summary>
    public sealed class ApiOptions
    {
        /// <summary>
        /// Gets or sets the CORS options for the API.
        /// </summary>
        public ApiCorsOptions? Cors { get; set; }

        /// <summary>
        /// Gets or sets the documentation options for the API.
        /// </summary>
        public DocumentationOptions? Documentation { get; set; }

        /// <summary>
        /// Gets or sets the license options for the API.
        /// </summary>
        public LicenseOptions? License { get; set; }
    }
}
