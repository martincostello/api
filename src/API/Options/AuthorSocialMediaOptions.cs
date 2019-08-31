﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Options
{
    /// <summary>
    /// A class representing the social media options for the site author. This class cannot be inherited.
    /// </summary>
    public sealed class AuthorSocialMediaOptions
    {
        /// <summary>
        /// Gets or sets the Facebook profile Id of the author.
        /// </summary>
        public string Facebook { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Twitter handle of the author.
        /// </summary>
        public string Twitter { get; set; } = string.Empty;
    }
}
