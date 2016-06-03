// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeResponseExampleProvider.cs" company="https://martincostello.com/">
//   Martin Costello (c) 2016
// </copyright>
// <summary>
//   TimeResponseExampleProvider.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MartinCostello.Api.Swagger
{
    using System;
    using Models;

    /// <summary>
    /// A class representing an implementation of <see cref="IExampleProvider"/>
    /// for the <see cref="TimeResponse"/> class. This class cannot be inherited.
    /// </summary>
    public sealed class TimeResponseExampleProvider : IExampleProvider
    {
        /// <inheritdoc />
        public object GetExample()
        {
            return new TimeResponse()
            {
                Timestamp = new DateTimeOffset(2016, 6, 3, 18, 44, 14, TimeSpan.Zero),
                Rfc1123 = "Fri, 03 Jun 2016 18:44:14 GMT",
                UniversalFull = "Friday, 03 June 2016 18:44:14",
                UniversalSortable = "2016-06-03 18:44:14Z",
                Unix = 1464979454,
            };
        }
    }
}
