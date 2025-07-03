// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using Grpc.Core;

namespace MartinCostello.Api;

/// <summary>
/// Represents a gRPC service for providing the current time.
/// </summary>
/// <param name="timeProvider">The <see cref="TimeProvider"/> to use.</param>
internal sealed class TimeGrpcService(TimeProvider timeProvider) : Time.TimeBase
{
    /// <summary>
    /// Gets the current time as a gRPC response.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="context">The <see cref="ServerCallContext"/>.</param>
    /// <returns>
    /// A <see cref="Task{TimeReply}"/> containing the current time response.
    /// </returns>
    public override Task<TimeReply> Now(TimeRequest request, ServerCallContext context)
    {
        var now = TimeService.Now(timeProvider);

        var result = new TimeReply()
        {
            Rfc1123 = now.Rfc1123,
            Timestamp = now.Timestamp.ToString("O", CultureInfo.InvariantCulture),
            UniversalFull = now.UniversalFull,
            UniversalSortable = now.UniversalSortable,
            Unix = now.Unix,
        };

        return Task.FromResult(result);
    }
}
