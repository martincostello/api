// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// An <see cref="IModelBinder"/> implementation to bind models of type <see cref="CancellationToken"/>
    /// that also sets a global request timeout for the cancellation token. This class cannot be inherited.
    /// </summary>
    public sealed class CustomCancellationTokenModelBinder : IModelBinder
    {
        /// <inheritdoc />
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var deadline = bindingContext.HttpContext.RequestServices.GetRequiredService<RequestDeadline>();

            // We need to force boxing now, so we can insert the same reference to the boxed CancellationToken
            // in both the ValidationState and ModelBindingResult.
            // DO NOT simplify this code by removing the cast.
            object model = deadline.Token;
            bindingContext.ValidationState.Add(model, new ValidationStateEntry() { SuppressValidation = true });
            bindingContext.Result = ModelBindingResult.Success(model);

            return Task.CompletedTask;
        }
    }
}
