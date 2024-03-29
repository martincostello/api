// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

#pragma warning disable SA1600
#pragma warning disable SA1649

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.Api.Pages;

public class ErrorModel : PageModel
{
    public int ErrorStatusCode { get; set; } = StatusCodes.Status500InternalServerError;

    public void OnGet(int? id)
    {
        if (id is { } status && status >= 400 && status < 599)
        {
            ErrorStatusCode = status;
        }

        Response.StatusCode = ErrorStatusCode;
    }
}
