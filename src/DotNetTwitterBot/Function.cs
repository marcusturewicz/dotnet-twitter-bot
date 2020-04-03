using System;
using System.Collections.Generic;
using System.Linq;
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

            var searchTerms = new[] { "\".NET Framework\"", "\".NET Core\"", "\".NET 5\"", "dotnet", "dotnetcore", "_dotnetbot_" };

            var searchSince = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10));

            var filterTerms = new[] { "domain", "registration", "domainregistration", "@paul_dotnet" };

            var me = User.GetAuthenticatedUser();

            foreach (var term in searchTerms)
            {
                var param = new SearchTweetsParameters(term)
                {
                    Since = searchSince,
                    TweetSearchType = TweetSearchType.OriginalTweetsOnly,
                    Filters = TweetSearchFilters.Safe
                };

                var tweets = Search.SearchTweets(param);

                foreach (var tweet in tweets)
                {
                    // Exclude tweets that don't specifically mention search terms
                    if (!tweet.Text.Contains(term, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Exclude tweets that contain excluded words.
                    if (filterTerms.Any(d => tweet.Text.Contains(d, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    // Exclude tweets that are from automated GitHub issues, except dotnetissues because
                    // it aggregates them rather than having one separate account for each.
                    if (tweet.CreatedBy.ScreenName.EndsWith("issues", StringComparison.OrdinalIgnoreCase)
                        && !tweet.CreatedBy.ScreenName.EndsWith("dotnetissues", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var retweetTask = tweet.PublishRetweetAsync();
                    var followTask = me.FollowUserAsync(tweet.CreatedBy.Id);

                    await retweetTask;
                    await followTask;
                }
            }
        }
    }
}
