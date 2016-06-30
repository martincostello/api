// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A class representing the controller for the <c>/</c> resource.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Gets the view for the home page.
        /// </summary>
        /// <returns>
        /// The view for the home page.
        /// </returns>
        [HttpGet]
        public IActionResult Index() => View();
    }
}
