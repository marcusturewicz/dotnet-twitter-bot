using System;
using Amazon.Lambda.Core;

namespace DotNetTwitterBot
{
    public class Functions
    {
        public Functions()
        {
        }

        public void Retweet(ILambdaContext context)
        {
            context.Logger.LogLine($"Function executed at {DateTime.UtcNow}\n");
        }
    }
}
