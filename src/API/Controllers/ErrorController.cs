// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.Api.Controllers
{
    /// <summary>
    /// A class representing the controller for the <c>/error</c> resource.
    /// </summary>
    public class ErrorController : Controller
    {
        /// <summary>
        /// A random set of annoying YouTube videos. This field is read-only.
        /// </summary>
        /// <remarks>
        /// Inspired by <c>https://gist.github.com/NickCraver/c9458f2e007e9df2bdf03f8a02af1d13</c>.
        /// </remarks>
        private static readonly string[] Videos =
        {
            "https://www.youtube.com/watch?v=wbby9coDRCk",
            "https://www.youtube.com/watch?v=nb2evY0kmpQ",
            "https://www.youtube.com/watch?v=eh7lp9umG2I",
            "https://www.youtube.com/watch?v=z9Uz1icjwrM",
            "https://www.youtube.com/watch?v=Sagg08DrO5U",
            "https://www.youtube.com/watch?v=jI-kpVh6e1U",
            "https://www.youtube.com/watch?v=jScuYd3_xdQ",
            "https://www.youtube.com/watch?v=S5PvBzDlZGs",
            "https://www.youtube.com/watch?v=9UZbGgXvCCA",
            "https://www.youtube.com/watch?v=O-dNDXUt1fg",
            "https://www.youtube.com/watch?v=MJ5JEhDy8nE",
            "https://www.youtube.com/watch?v=VnnWp_akOrE",
            "https://www.youtube.com/watch?v=jwGfwbsF4c4",
            "https://www.youtube.com/watch?v=8ZcmTl_1ER8",
            "https://www.youtube.com/watch?v=gLmcGkvJ-e0",
            "https://www.youtube.com/watch?v=clU0Sh9ngmY",
            "https://www.youtube.com/watch?v=sCNrK-n68CM",
            "https://www.youtube.com/watch?v=hgwpZvTWLmE",
            "https://www.youtube.com/watch?v=jAckVuEY_Rc",
            "https://www.youtube.com/watch?v=NkWkHBxfXf0",
            "https://www.youtube.com/watch?v=DoyBgTAZrFw",
        };

        /// <summary>
        /// Gets the result for the <c>/error</c> action.
        /// </summary>
        /// <param name="id">The optional HTTP status code associated with the error.</param>
        /// <returns>
        /// The result for the <c>/error</c> action.
        /// </returns>
        [HttpGet]
        public IActionResult Index(int? id)
        {
            Response.StatusCode = id ?? StatusCodes.Status500InternalServerError;

            if (id < 400 || id > 599)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
            }

            return View("Error", Response.StatusCode);
        }

        /// <summary>
        /// Gets the result for various routes that scrapers probe.
        /// </summary>
        /// <returns>
        /// The result for the action.
        /// </returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [HttpHead]
        [HttpPost]
        [Route("account/login")]
        [Route("admin.php")]
        [Route("admin-console")]
        [Route("admin/{*catchall}")]
        [Route("administrator/{*catchall}")]
        [Route("ajaxproxy/{*catchall}")]
        [Route("bitrix/{*catchall}")]
        [Route("blog/{*catchall}")]
        [Route("cms/{*catchall}")]
        [Route("index.php")]
        [Route("invoker/{*catchall}")]
        [Route("jmx-console/{*catchall}")]
        [Route("license.php")]
        [Route("magmi/{*catchall}")]
        [Route("manager/{*catchall}")]
        [Route("modules/{*catchall}")]
        [Route("phpmyadmin")]
        [Route("readme.html")]
        [Route("site/{*catchall}")]
        [Route("sites/{*catchall}")]
        [Route("tiny_mce/{*catchall}")]
        [Route("uploadify/{*catchall}")]
        [Route("web-console/{*catchall}")]
        [Route("wordpress/{*catchall}")]
        [Route("wp/{*catchall}")]
        [Route("wp-admin/{*catchall}")]
        [Route("wp-content/{*catchall}")]
        [Route("wp-includes/{*catchall}")]
        [Route("wp-links-opml.php")]
        [Route("wp-login.php")]
        [Route("xmlrpc.php")]
        public ActionResult No() => Redirect(Videos[RandomNumberGenerator.GetInt32(0, Videos.Length)]);
    }
}
