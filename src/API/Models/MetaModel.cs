// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetaModel.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   MetaModel.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;

namespace MartinCostello.Api.Models
{
    /// <summary>
    /// A class representing the view model for page metadata. This class cannot be inherited.
    /// </summary>
    public sealed class MetaModel
    {
        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the canonical URI.
        /// </summary>
        public string CanonicalUri { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Facebook profile Id.
        /// </summary>
        public string Facebook { get; set; }

        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the image URI.
        /// </summary>
        public string ImageUri { get; set; }

        /// <summary>
        /// Gets or sets the image alternate text.
        /// </summary>
        public string ImageAltText { get; set; }

        /// <summary>
        /// Gets or sets the page keywords.
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// Gets or sets the robots value.
        /// </summary>
        public string Robots { get; set; }

        /// <summary>
        /// Gets or sets the site name.
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets the site type.
        /// </summary>
        public string SiteType { get; set; }

        /// <summary>
        /// Gets or sets the page title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Twitter card type.
        /// </summary>
        public string TwitterCard { get; set; }

        /// <summary>
        /// Gets or sets the Twitter handle.
        /// </summary>
        public string TwitterHandle { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="MetaModel"/>.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        /// <param name="canonicalUri">The optional canonical URI of the page.</param>
        /// <param name="hostName">The optional host name.</param>
        /// <param name="description">The optional page description.</param>
        /// <param name="imageUri">The optional image URI.</param>
        /// <param name="imageAltText">The optional image alternate text.</param>
        /// <param name="robots">The optional robots value.</param>
        /// <param name="title">The optional page title.</param>
        /// <returns>
        /// The created instance of <see cref="MetaModel"/>.
        /// </returns>
        public static MetaModel Create(
            IConfiguration config,
            string canonicalUri = null,
            string hostName = null,
            string description = null,
            string imageUri = null,
            string imageAltText = null,
            string robots = null,
            string title = null)
        {
            return new MetaModel()
            {
                Author = config["Site:Metadata:Author:Name"],
                CanonicalUri = canonicalUri ?? string.Empty,
                Description = description ?? config["Site:Metadata:Description"],
                Facebook = config["Site:Metadata:Author:SocialMedia:Facebook"],
                HostName = config["Site:Metadata:Domain"],
                ImageUri = imageUri ?? string.Empty,
                ImageAltText = imageAltText ?? string.Empty,
                Keywords = config["Site:Metadata:Keywords"] ?? "martin,costello,api",
                Robots = robots ?? config["Site:Metadata:Robots"],
                SiteName = config["Site:Metadata:Name"] ?? "api.martincostello.com",
                SiteType = config["Site:Metadata:Type"] ?? "website",
                Title = title + " - " + config["Site:Metadata:Name"],
                TwitterCard = "summary",
                TwitterHandle = config["Site:Metadata:Author:SocialMedia:Twitter"],
            };
        }
    }
}
