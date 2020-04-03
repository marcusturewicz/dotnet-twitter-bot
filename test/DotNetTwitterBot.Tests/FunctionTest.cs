using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

using DotNetTwitterBot;

namespace DotNetTwitterBot.Tests
{
    public class FunctionTest
    {
        public FunctionTest()
        {
        }

        [Fact]
        public async Task TetGetMethod()
        {
            var functions = new Functions();
            var context = new TestLambdaContext();
            await functions.Retweet(context);
        }
    }
}
