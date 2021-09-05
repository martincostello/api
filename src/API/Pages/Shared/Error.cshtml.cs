// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

#pragma warning disable CS1591
#pragma warning disable SA1600
#pragma warning disable SA1649

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.Api.Pages
{
    public class ErrorModel : PageModel
    {
        public int ErrorStatusCode { get; set; } = StatusCodes.Status500InternalServerError;

        public void OnGet(int? id)
        {
            if (id.HasValue && (id >= 400 && id < 599))
            {
                ErrorStatusCode = id.Value;
            }

            Response.StatusCode = ErrorStatusCode;
        }
    }
}
