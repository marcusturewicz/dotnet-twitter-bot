using System;
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
        static readonly string[] SearchTerms = new[]
        {
            "\".NET Framework\"",
            "\".NET Core\"",
            "\".NET 5\""
        };

        static readonly string[] SearchTracks = new[]
        {
            "#dotnet",
            "#dotnetcore",
            "#dotnet5",
            "#dotnet6"
        };

        public async Task Retweet(ILambdaContext context)
        {
            var creds = await SecretHelper.GetSecretAsync();
            Auth.SetUserCredentials(creds.ConsumerKey, creds.ConsumerSecret, creds.AccessToken, creds.AccessSecret);
            
            var searchSince = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(35));
            var me = User.GetAuthenticatedUser();

            await Task.WhenAll(
                SearchAndRetweetTweets(SearchTerms, searchSince, me),
                SearchAndRetweetTweets(SearchTracks, searchSince, me));

            static async Task SearchAndRetweetTweets(string[] terms, DateTime searchSince, IAuthenticatedUser me)
            {
                var filterTerms = new[] { "domain", "registration", "domainregistration", "@paul_dotnet" };
                var query = string.Join(" OR ", terms);
                var param = new SearchTweetsParameters(query)
                {
                    Since = searchSince,
                    TweetSearchType = TweetSearchType.OriginalTweetsOnly,
                    Filters = TweetSearchFilters.Safe
                };

                var tweets = await SearchAsync.SearchTweets(param);
                foreach (var tweet in tweets)
                {
                    // Exclude tweets that don't specifically mention search terms
                    if (!terms.Any(term => tweet.Text.Contains(term, StringComparison.OrdinalIgnoreCase)))
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

                    await Task.WhenAll(retweetTask, followTask);
                }
            }
        }
    }
}
