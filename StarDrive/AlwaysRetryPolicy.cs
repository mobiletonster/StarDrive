using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDrive
{
    internal class AlwaysRetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            var retryDelay = Math.Min(2500, 50 * 2 ^ retryContext.PreviousRetryCount) + new Random().Next(100);
            return TimeSpan.FromMilliseconds(retryDelay);
        }
    }
}
