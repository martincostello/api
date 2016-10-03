// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Middleware
{
    using System.Threading.Tasks;
    using AspNetCoreRateLimit;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// A class representing IP rate-limiting middleware. This class cannot be inherited.
    /// </summary>
    public sealed class CustomIpRateLimitMiddleware : IpRateLimitMiddleware
    {
        private readonly IpRateLimitOptions _options;
        private readonly JsonSerializerSettings _serializerSettings;

        /// <inheritdoc />
        public CustomIpRateLimitMiddleware(RequestDelegate next, IOptions<IpRateLimitOptions> options, IRateLimitCounterStore counterStore, IIpPolicyStore policyStore, ILogger<IpRateLimitMiddleware> logger, IIpAddressParser ipParser = null)
            : base(next, options, counterStore, policyStore, logger, ipParser)
        {
            _options = options.Value;

            // TODO It would be better to get this through DI using
            // the same serializer settings as MVC is using.
            _serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        /// <inheritdoc />
        public override async Task ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitRule rule, string retryAfter)
        {
            var headers = httpContext.Response.GetTypedHeaders();

            headers.ContentType = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            headers.Append("Retry-After", retryAfter);

            httpContext.Response.StatusCode = _options.HttpStatusCode;

            var response = new
            {
                message = "API rate limit exceeded.",
                status = _options.HttpStatusCode,
            };

            var json = JsonConvert.SerializeObject(response, _serializerSettings);

            await httpContext.Response.WriteAsync(json);
        }
    }
}
