// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

using MartinCostello.Api.Models;

namespace MartinCostello.Api;

/// <summary>
/// Represents a gRPC service for providing the current time.
/// </summary>
public static class TimeService
{
    /// <summary>
    /// Gets the current time as a gRPC response.
    /// </summary>
    /// <param name="timeProvider">The <see cref="TimeProvider"/> to use.</param>
    /// <returns>
    /// A <see cref="Task{TimeResponse}"/> containing the current time response.
    /// </returns>
    public static TimeResponse Now(TimeProvider timeProvider)
    {
        var formatProvider = CultureInfo.InvariantCulture;
        var now = timeProvider.GetUtcNow();

        return new TimeResponse()
        {
            Timestamp = now,
            Rfc1123 = now.ToString("r", formatProvider),
            UniversalFull = now.UtcDateTime.ToString("U", formatProvider),
            UniversalSortable = now.UtcDateTime.ToString("u", formatProvider),
            Unix = now.ToUnixTimeSeconds(),
        };
    }
}
