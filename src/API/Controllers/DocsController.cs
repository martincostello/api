// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.Api.Controllers
{
    /// <summary>
    /// A class representing the controller for the <c>/docs/</c> resource.
    /// </summary>
    public class DocsController : Controller
    {
        /// <summary>
        /// Gets the view for the documentation page.
        /// </summary>
        /// <returns>
        /// The view for the documentation page.
        /// </returns>
        [HttpGet]
        public IActionResult Index() => View();
    }
}
