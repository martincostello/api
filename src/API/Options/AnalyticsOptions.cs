﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Options
{
    /// <summary>
    /// A class representing the analytics options for the site. This class cannot be inherited.
    /// </summary>
    public sealed class AnalyticsOptions
    {
        /// <summary>
        /// Gets or sets the Google property Id.
        /// </summary>
        public string Google { get; set; } = string.Empty;
    }
}
