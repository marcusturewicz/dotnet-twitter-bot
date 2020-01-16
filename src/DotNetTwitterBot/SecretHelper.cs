using System;
using System.IO;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;

namespace DotNetTwitterBot
{
    public class SecretHelper
    {
        public static async Task<Secrets> GetSecretAsync()
        {
            string secretName = "dotnet-twitter-bot";
            string region = "ap-southeast-2";

            using (var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region)))
            {
                var request = new GetSecretValueRequest() { SecretId = secretName };
                GetSecretValueResponse response = await client.GetSecretValueAsync(request);
                string secret = null;

                if (response.SecretString != null)
                {
                    secret = response.SecretString;
                }
                else
                {
                    using (var reader = new StreamReader(response.SecretBinary))
                    {
                        secret = Encoding.UTF8.GetString(Convert.FromBase64String(await reader.ReadToEndAsync()));
                    }
                }

                return JsonConvert.DeserializeObject<Secrets>(secret);
            }
        }
    }

    public class Secrets
    {
        [JsonProperty("TWITTER_CONSUMER_KEY")]
        public string ConsumerKey { get; set; }
        [JsonProperty("TWITTER_CONSUMER_SECRET_KEY")]
        public string ConsumerSecret { get; set; }
        [JsonProperty("TWITTER_ACCESS_TOKEN")]
        public string AccessToken { get; set; }
        [JsonProperty("TWITTER_ACCESS_TOKEN_SECRET")]
        public string AccessSecret { get; set; }
    }
}
