using System;
using Amazon.Lambda.Core;
using Tweetinvi;
using Tweetinvi.Parameters;

namespace DotNetTwitterBot
{
    public class Functions
    {
        public Functions()
        {
        }

        public void Retweet(ILambdaContext context)
        {
            context.Logger.LogLine($"Function executed at {DateTime.UtcNow}");

            Auth.SetUserCredentials(
                Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY"),
                Environment.GetEnvironmentVariable("TWITTER_CONSUMER_SECRET_KEY"),
                Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN"),
                Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN_SECRET"));

            var searchTerms = new[] { ".NET Framework", ".NET Core", "dotnet", "dotnetcore", ".NET 5" };

            var searchSince = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(30));
            
            var me = User.GetAuthenticatedUser();

            foreach (var term in searchTerms)
            {
                var param = new SearchTweetsParameters(term) { Since = searchSince };
                var tweets = Search.SearchTweets(param);
                foreach (var tweet in tweets)
                {
                    try
                    {
                        Tweet.PublishRetweet(tweet.Id);
                        me.FollowUser(tweet.CreatedBy.Id);
                    }
                    catch (Exception e)
                    {
                        context.Logger.LogLine($"Exception occured: {e.Message}");
                    }
                }
            }
        }
    }
}

