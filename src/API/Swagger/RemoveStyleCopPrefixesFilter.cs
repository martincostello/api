// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveStyleCopPrefixesFilter.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   RemoveStyleCopPrefixesFilter.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MartinCostello.Api.Swagger
{
    using System;
    using Swashbuckle.SwaggerGen.Generator;

    /// <summary>
    /// A class representing an operation filter that modifies XML documentation that matches <c>StyleCop</c>
    /// requirements to be more human-readable for display in Swagger documentation. This class cannot be inherited.
    /// </summary>
    internal sealed class RemoveStyleCopPrefixesFilter : IOperationFilter
    {
        /// <summary>
        /// The documentation prefix to remove.
        /// </summary>
        private const string Prefix = "Gets or sets ";

        /// <inheritdoc />
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context?.SchemaRegistry?.Definitions != null)
            {
                foreach (var definition in context.SchemaRegistry.Definitions.Values)
                {
                    if (definition.Properties != null)
                    {
                        foreach (var property in definition.Properties.Values)
                        {
                            if (property.Description != null)
                            {
                                if (property.Description.StartsWith(Prefix, StringComparison.Ordinal))
                                {
                                    // Remove the StyleCop property prefix
                                    property.Description = property.Description.Replace(Prefix, string.Empty);

                                    // Capitalize the first letter that's left over
                                    property.Description = char.ToUpperInvariant(property.Description[0]) + property.Description.Substring(1);
                                }

                                // Swagger displays properties as a comma-separated list, so remove the '.' as otherwise it looks odd
                                property.Description = property.Description.TrimEnd('.');
                            }
                        }
                    }
                }
            }
        }
    }
}
