// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api
{
    using System;
    using System.Threading;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// A custom <see cref="IModelBinderProvider"/> for <see cref="CancellationToken"/> that
    /// sets a global request timeout for the cancellation token. This class cannot be inherited.
    /// </summary>
    public sealed class CustomCancellationTokenModelBinderProvider : IModelBinderProvider
    {
        /// <summary>
        /// The <see cref="CustomCancellationTokenModelBinder"/> to use.
        /// </summary>
        private CustomCancellationTokenModelBinder? _modelBinder;

        /// <inheritdoc />
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(CancellationToken))
            {
                return _modelBinder ??= new CustomCancellationTokenModelBinder();
            }

            return null;
        }
    }
}
