// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpRequestExtensions.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   HttpRequestExtensions.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace MartinCostello.Api.Extensions
{
    /// <summary>
    /// A class containing extension methods for the <see cref="HttpRequest"/> class. This class cannot be inherited.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Returns the canonical URI for the specified HTTP request with the optional path.
        /// </summary>
        /// <param name="request">The HTTP request to get the canonical URI from.</param>
        /// <param name="path">The optional path to get the canonical URI for.</param>
        /// <returns>
        /// The canonical URI to use for the specified HTTP request.
        /// </returns>
        public static string Canonical(this HttpRequest request, string path = null)
        {
            string host = request.Host.ToString();
            string[] hostSplit = host.Split(':');

            UriBuilder builder = new UriBuilder();
            builder.Host = hostSplit[0];

            if (hostSplit.Length > 1)
            {
                builder.Port = int.Parse(hostSplit[1], CultureInfo.InvariantCulture);
            }

            builder.Path = path ?? request.Path;
            builder.Query = string.Empty;
            builder.Scheme = "https";

            string canonicalUri = builder.Uri.AbsoluteUri.ToLowerInvariant();

            if (!canonicalUri.EndsWith("/"))
            {
                canonicalUri += "/";
            }

            return canonicalUri;
        }
    }
}
