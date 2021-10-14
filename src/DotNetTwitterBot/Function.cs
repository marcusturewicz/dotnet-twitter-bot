using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.ML.OnnxRuntime;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace DotNetTwitterBot
{
    public class Functions
    {
        static readonly string[] SearchTerms = new[]
        {
            "\".NET AND Framework\"",
            "\".NET AND Core\"",
            "\".NET AND 5\"",
            "\".NET AND 6\"",
            "\".NET AND 7\"",
            "#dotnet",
            "#dotnetcore",
            "#dotnet5",
            "#dotnet6",
            "#dotnet7"
        };

        public async Task Retweet(ILambdaContext context)
        {
            var creds = await SecretHelper.GetSecretAsync();
            Auth.SetUserCredentials(creds.ConsumerKey, creds.ConsumerSecret, creds.AccessToken, creds.AccessSecret);

            var searchSince = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(31));
            var me = User.GetAuthenticatedUser();

            await SearchAndRetweetTweets(SearchTerms, searchSince, me);

            static async Task SearchAndRetweetTweets(string[] terms, DateTime searchSince, IAuthenticatedUser me)
            {
                var query = string.Join(" OR ", terms);
                var param = new SearchTweetsParameters(query)
                {
                    Since = searchSince,
                    TweetSearchType = TweetSearchType.OriginalTweetsOnly,
                    Filters = TweetSearchFilters.Safe,
                    MaximumNumberOfResults = 1000
                };

                var tweets = await SearchAsync.SearchTweets(param);

                var spamFilter = new SpamFilter("spam_filter.onnx");

                var isSpam = spamFilter.Run(tweets.Select(t => $"RT @{t.CreatedBy.ScreenName} : {t.Text}")).ToArray();

                var tasks = tweets.Select(async (t, i) => {
                    if (!isSpam[i])
                        await t.PublishRetweetAsync();
                });

                await Task.WhenAll(tasks);
            }
        }
    }
}
