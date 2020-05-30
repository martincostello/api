// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api
{
    using System;
    using System.Threading;
    using MartinCostello.Api.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// A class representing a deadline for the completion of an HTTP request. This class cannot be inherited.
    /// </summary>
    public sealed class RequestDeadline : IDisposable
    {
        /// <summary>
        /// The <see cref="CancellationTokenSource"/> for the server deadline. This field is read-only.
        /// </summary>
        private readonly CancellationTokenSource _deadline;

        /// <summary>
        /// The linked <see cref="CancellationTokenSource"/> for the client and server. This field is read-only.
        /// </summary>
        private readonly CancellationTokenSource _linked;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestDeadline"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> to use.</param>
        /// <param name="options">The <see cref="IOptionsSnapshot{T}"/> to use.</param>
        public RequestDeadline(
            IHttpContextAccessor httpContextAccessor,
            IOptionsSnapshot<SiteOptions> options)
        {
            _deadline = new CancellationTokenSource(options.Value.RequestTimeout);
            _linked = CancellationTokenSource.CreateLinkedTokenSource(_deadline.Token, httpContextAccessor.HttpContext.RequestAborted);
        }

        /// <summary>
        /// Gets the <see cref="CancellationToken"/> representing the request deadline.
        /// </summary>
        public CancellationToken Token => _linked.Token;

        /// <inheritdoc />
        public void Dispose()
        {
            _deadline?.Dispose();
            _linked?.Dispose();
        }
    }
}
