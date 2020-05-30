// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api
{
    using System;
    using System.Threading;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// A custom <see cref="IModelBinderProvider"/> for <see cref="CancellationToken"/> that
    /// sets a global request timeout for the cancellation token. This class cannot be inherited.
    /// </summary>
    public sealed class CustomCancellationTokenModelBinderProvider : IModelBinderProvider
    {
        /// <summary>
        /// The <see cref="CustomCancellationTokenModelBinder"/> to use. This field is read-only.
        /// </summary>
        private readonly CustomCancellationTokenModelBinder _modelBinder;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomCancellationTokenModelBinderProvider"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> to use.</param>
        public CustomCancellationTokenModelBinderProvider(IHttpContextAccessor httpContextAccessor)
        {
            _modelBinder = new CustomCancellationTokenModelBinder(httpContextAccessor);
        }

        /// <inheritdoc />
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(CancellationToken))
            {
                return _modelBinder;
            }

            return null;
        }
    }
}
