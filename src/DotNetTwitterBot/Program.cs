using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using DotNetTwitterBot;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

var handler = async (ILambdaContext context) =>
{
    string[] searchTerms = new[]
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

    var creds = await SecretHelper.GetSecretAsync();
    Auth.SetUserCredentials(creds.ConsumerKey, creds.ConsumerSecret, creds.AccessToken, creds.AccessSecret);

    var searchSince = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(31));
    var me = User.GetAuthenticatedUser();

    await SearchAndRetweetTweets(searchTerms, searchSince, me);

    async Task SearchAndRetweetTweets(string[] terms, DateTime searchSince, IAuthenticatedUser me)
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
};

await LambdaBootstrapBuilder.Create(handler).Build().RunAsync();
