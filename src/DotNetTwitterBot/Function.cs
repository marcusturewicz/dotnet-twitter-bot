using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Tweetinvi.Parameters.Enum;

namespace DotNetTwitterBot
{
    public class Functions
    {
        static readonly string[] SearchTerms = new[]
        {
            "\".NET Framework\"",
            "\".NET Core\"",
            "\".NET 5\"",
            "\".NET 6\""
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
            var client = new TwitterClient(creds.ConsumerKey, creds.ConsumerSecret, creds.AccessToken, creds.AccessSecret);
            
            var searchSince = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(35));
            var me = await client.Users.GetAuthenticatedUserAsync();

            await Task.WhenAll(
                SearchAndRetweetTweets(client, SearchTerms, searchSince, me),
                SearchAndRetweetTweets(client, SearchTracks, searchSince, me));

            static async Task SearchAndRetweetTweets(TwitterClient client,string[] terms, DateTime searchSince, IAuthenticatedUser me)
            {
                var filterTerms = new[] { "domain", "registration", "domainregistration", "@paul_dotnet" };
                var query = string.Join(" OR ", terms);
                var param = new SearchTweetsParameters(query)
                {
                    Since = searchSince,
                    Filters = TweetSearchFilters.Safe
                };

                var tweets = await client.Search.SearchTweetsAsync(param);
                tweets = client.Search.FilterTweets(tweets, OnlyGetTweetsThatAre.OriginalTweets, false);
                foreach (var tweet in tweets)
                {
                    // Exclude tweets that contain excluded words.
                    if (filterTerms.Any(d => tweet.Text.Contains(d, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    var retweetTask = tweet.PublishRetweetAsync();
                    var followTask = me.FollowUserAsync(tweet.CreatedBy.Id);

                    await Task.WhenAll(retweetTask, followTask);
                }
            }
        }
    }
}
