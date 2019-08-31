﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Options
{
    /// <summary>
    /// A class representing the author options for the site. This class cannot be inherited.
    /// </summary>
    public sealed class AuthorOptions
    {
        /// <summary>
        /// Gets or sets the email address of the author.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the author.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the social media options.
        /// </summary>
        public AuthorSocialMediaOptions? SocialMedia { get; set; }

        /// <summary>
        /// Gets or sets the URL of the author's website.
        /// </summary>
        public string Website { get; set; } = string.Empty;
    }
}
