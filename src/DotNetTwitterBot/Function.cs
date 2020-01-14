using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace DotNetTwitterBot
{
    public class Functions
    {
        public Functions()
        {
        }

        public async Task Retweet(ILambdaContext context)
        {
            var creds = await SecretHelper.GetSecretAsync();

            Auth.SetUserCredentials(creds.ConsumerKey, creds.ConsumerSecret, creds.AccessToken, creds.AccessSecret);

            var searchTerms = new[] { ".NET Framework", ".NET Core", "dotnet", "dotnetcore", ".NET 5" };

            var searchSince = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(15));

            var me = User.GetAuthenticatedUser();

            foreach (var term in searchTerms)
            {
                var param = new SearchTweetsParameters(term)
                {
                    Since = searchSince,
                    TweetSearchType = TweetSearchType.OriginalTweetsOnly
                };

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
