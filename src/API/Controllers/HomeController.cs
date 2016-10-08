// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Controllers
{
    using System;
    using System.Linq;
    using Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A class representing the controller for the <c>/</c> resource.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// An array containing a single forward-slash character. This field is read-only.
        /// </summary>
        private static readonly char[] Slash = new[] { '/' };

        /// <summary>
        /// Gets the view for the home page.
        /// </summary>
        /// <returns>
        /// The view for the home page.
        /// </returns>
        [HttpGet]
        public IActionResult Index()
        {
            if (IsJsonRequest())
            {
                var rootUri = new Uri(Request.Canonical(), UriKind.Absolute);

                var data = new
                {
                    time_url = ApiUrl("time", "get", rootUri),
                    generate_guid_url = ApiUrl("tools", "guid", rootUri),
                    generate_hash_url = ApiUrl("tools", "hash", rootUri),
                    generate_machine_key_url = ApiUrl("tools", "machinekey", rootUri),
                };

                return Json(data);
            }
            else
            {
                return View();
            }
        }

        /// <summary>
        /// Returns whether the current request accepts JSON.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the current request accepts JSON; otherwise <see langword="false"/>.
        /// </returns>
        private bool IsJsonRequest()
        {
            string mediaType = Request.GetTypedHeaders().Accept?.FirstOrDefault()?.MediaType;
            return string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the absolute URI of the specified API controller method.
        /// </summary>
        /// <param name="controller">The name of the controller.</param>
        /// <param name="action">The name of the action method.</param>
        /// <param name="rootUri">The root URI.</param>
        /// <returns>
        /// The absolute URI of the specified API resource.
        /// </returns>
        private string ApiUrl(string controller, string action, Uri rootUri)
        {
            return new Uri(rootUri, Url.Action(action, controller)).AbsoluteUri.TrimEnd(Slash);
        }
    }
}
