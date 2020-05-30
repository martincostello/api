// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Middleware
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MartinCostello.Api.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// A middleware that enforces a maximum request execution deadline. This class cannot be inherited.
    /// </summary>
    public sealed class RequestDeadlineMiddleware
    {
        /// <summary>
        /// The delegate for the next part of the pipeline. This field is read-only.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestDeadlineMiddleware"/> class.
        /// </summary>
        /// <param name="next">The delegate for the next part of the pipeline.</param>
        public RequestDeadlineMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware asynchronously.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the actions performed by the middleware.
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            CancellationToken requestAborted = context.RequestAborted;

            var timeout = context.RequestServices.GetRequiredService<IOptions<SiteOptions>>().Value.RequestTimeout;

            using var deadline = new CancellationTokenSource(timeout);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(deadline.Token, context.RequestAborted);

            context.RequestAborted = cts.Token;

            try
            {
                var delay = Task.Delay(timeout, cts.Token);
                var next = _next.Invoke(context);

                if (await Task.WhenAny(delay, next) == delay && !context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                }

                cts.Cancel();
                deadline.Cancel();
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
            {
                context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
            }
            finally
            {
                context.RequestAborted = requestAborted;
            }
        }
    }
}
