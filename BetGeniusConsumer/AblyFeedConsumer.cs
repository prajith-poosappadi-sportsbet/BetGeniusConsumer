using temp = Microsoft.Extensions.Configuration;
using IO.Ably;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BetGeniusConsumer.Helpers;
using BetGeniusConsumer.Interfaces;
using BetGeniusConsumer.Secrets;

namespace BetGeniusConsumer
{
    public class AblyFeedConsumer : IConsumer
    {
        private readonly CancellationTokenSource _source;
        private readonly ILogger _logger;
        private readonly IAnalysis _analysis;
        private readonly ISecretRevealer _secretRevealer;

        public AblyFeedConsumer(ILogger logger, IAnalysis analysis, ISecretRevealer secretRevealer)
        {
            Console.CancelKeyPress += CancelKeyPressHandler;
            _source = new CancellationTokenSource();
            _logger = logger;
            _analysis = analysis;
            _secretRevealer = secretRevealer;
        }

        private void CancelKeyPressHandler(object? sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            _source.Cancel();
        }

        public async Task Listen()
        {
            try
            {
                await SubscribeToChannel();
            }
            catch (TaskCanceledException ex)
            {
                _logger.StoreLogs($"Application Terminated: {ex.Message}", true);
            }
            finally
            {
                _source.Dispose();
            }
        }

        private async Task SubscribeToChannel()
        {
            _logger.StoreLogs("**************************************************************");
            _logger.StoreLogs("Subscribing to the Ably Channel");
            await Subscribe();
        }

        private async Task<(string? channelName, string? accessToken)> GetAblyFeed()
        {
            var accessToken = GetGeniusSportsBearerToken().Result;
            
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.StoreLogs("Token was not provided due to an error", true);
                return ("", "");
            }
            
            var fixtureId = GetFixtureId(accessToken).Result;

            if (string.IsNullOrWhiteSpace(fixtureId))
            {
                _logger.StoreLogs("Fixture not available during the given duration. Please edit the timestamps and try again", true);
                return ("", "");
            }

            var accessUrl =
                $"{FixtureSpecifics.BaseUrl}/sources/{FixtureSpecifics.SourceId}/sports/{FixtureSpecifics.SportId}/fixtures/{fixtureId}/liveaccess";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);
            httpClient.DefaultRequestHeaders.Add("x-api-key", _secretRevealer.Reveal().ApiKey);
            var accessResponse = await httpClient.GetAsync(accessUrl);
            var accessResponseBody = await accessResponse.Content.ReadAsStringAsync();

            var ablyFeed = JsonConvert.DeserializeObject<JObject>(accessResponseBody);
            var ablyChannelName = ablyFeed?["channelName"]?.ToString();
            var ablyAccessToken = ablyFeed?["accessToken"]?.ToString();
            return (ablyChannelName, ablyAccessToken);
        }

        private async Task<object> AblyAuthCallback(TokenParams arg)
        {
            var (_, accessToken) = await GetAblyFeed();
            return new TokenDetails
            {
                Token = accessToken
            };
        }

        private async Task<string?> GetGeniusSportsBearerToken()
        {
            var body = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _secretRevealer.Reveal().ClientId },
                { "client_secret", _secretRevealer.Reveal().ClientSecret }
            };

            var accessToken = "";

            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(FixtureSpecifics.AuthUrl, new FormUrlEncodedContent(body));
                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenEnvelope = JsonConvert.DeserializeObject<JObject>(responseBody);
                accessToken = tokenEnvelope?["access_token"]?.ToString();
            }
            catch (Exception ex)
            {
                _logger.StoreLogs($"GetGeniusSportsBearerToken method Exception: {ex}", true);
            }

            return accessToken;
        }

        private async Task<string?> GetFixtureId(string accessToken)
        {
            var fixtureUrl =
                $"{FixtureSpecifics.BaseUrl}/sources/{FixtureSpecifics.SourceId}/sports/{FixtureSpecifics.SportId}" +
                $"/schedule?from={FixtureSpecifics.FromDateTimeString}&to={FixtureSpecifics.ToDateTimeString}";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);
            httpClient.DefaultRequestHeaders.Add("x-api-key", _secretRevealer.Reveal().ApiKey);

            var fixtureId = "";

            try
            {
                var responseFixture = await httpClient.GetAsync(fixtureUrl);
                var responseFixtureBody = await responseFixture.Content.ReadAsStringAsync();

                var schedule = JsonConvert.DeserializeObject<JArray>(responseFixtureBody);
                
                // TODO: To get all fixtures Ids instead of just one
                fixtureId = schedule?[0]["fixtureId"]?.ToString();
            }
            catch (Exception ex)
            {
                _logger.StoreLogs($"GetFixtureId method Exception: {ex}", true);
            }

            return fixtureId;
        }

        private async Task Subscribe()
        {
            var (ablyChannelName, ablyAccessToken) = GetAblyFeed().Result;
            
            if (string.IsNullOrWhiteSpace(ablyChannelName) || string.IsNullOrWhiteSpace(ablyAccessToken))
            {
                _logger.StoreLogs("Error, please debug", true);
                return;
            }
            
            var ably = new AblyRealtime(new ClientOptions
            {
                Token = ablyAccessToken,
                AutoConnect = false,
                AuthCallback = AblyAuthCallback,
                Environment = "geniussports",
                FallbackHosts = new[]
                {
                    "geniussports-a-fallback.ably-realtime.com",
                    "geniussports-b-fallback.ably-realtime.com",
                    "geniussports-c-fallback.ably-realtime.com",
                    "geniussports-d-fallback.ably-realtime.com",
                    "geniussports-e-fallback.ably-realtime.com"
                }
            });
            
            _logger.StoreLogs($"Ably Channel Name: {ablyChannelName}");
            
            _logger.StoreLogs("-------------------------------------------------");
            
            var channelParams = new ChannelParams { { "delta", "vcdiff" } };
            var channelOptions = new ChannelOptions
            {
                Params = channelParams
            };
            var channel = ably.Channels.Get(ablyChannelName, channelOptions);
            
            channel.Subscribe(message =>
            {
                // _logger.StoreLogs($"Message name: {message.Name}");
                _logger.StoreLogs($"Message data size: {message.Data}");
                _logger.StoreLogs($"Message data: {message.Data}");
                _logger.StoreLogs("-------------------------------------------------");
                
                // Send for analysis
                _analysis.StoreForAnalysis(message.Data.ToString());
            });
            
            channel.On(args =>
            {
                var state = args.Current;
                var error = args.Error;
                _logger.StoreLogs($"channel state: {state}, error: {error}");
            });
        }

        private void GetSize<T>(T obj)
        {
            if (obj != null)
            {
                RuntimeTypeHandle th = obj.GetType().TypeHandle;
                int size = *(*(int**)&th + 1);
            }
        }
    }
}