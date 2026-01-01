using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SocketIOClient;
using SocketIOClient.Transport;
using System.Net.Http.Headers;
using Ubiq.Definitions.Http;
using Ubiq.Definitions.Market;
using Ubiq.Extensions;

namespace Ubiq.FourcastersAPI
{
    public class FourcastersAPI : IAsyncDisposable
    {
        private readonly ILogger<FourcastersAPI> m_Logger;
        private readonly IHttpClientHelper m_HttpClientHelper;
        private readonly HttpClient m_HttpClient;
        private readonly HttpClient m_HttpClientLongTimeout;
        private readonly Uri m_SocketUri;
        private readonly string m_BaseUrl;
        private readonly string m_Username;
        private readonly string m_Password;
        private readonly string m_Currency;
        private readonly decimal m_CommissionRate;

        private string m_Session;
        private SocketIOClient.SocketIO m_UserSocket;
        private SocketIOClient.SocketIO m_PublicSocket;
        private PriceFormat m_PriceFormat = PriceFormat.American;

        public event EventHandler UserWebSocketConnected;
        public event EventHandler UserWebSocketDisconnected;
        public event EventHandler PublicWebSocketConnected;
        public event EventHandler PublicWebSocketDisconnected;

        public event EventHandler<PositionUpdateMessage> PositionUpdated;
        public event EventHandler<GameUpdateMessage> GameUpdated;
        public event EventHandler<OrderUpdateMessage> OrderUpdated;

        public FourcastersAPI(ILogger<FourcastersAPI> logger, IHttpClientHelper httpClientExtensions, HttpClient httpClient, HttpClient httpClientLongTimeout, Uri baseUri, Uri socketUri, string username, string password, string currency = "USD", decimal commissionRate = 1.0m)
        {
            m_Logger = logger;
            m_HttpClientHelper = httpClientExtensions;
            m_HttpClient = httpClient;
            m_HttpClientLongTimeout = httpClientLongTimeout;
            m_SocketUri = socketUri;
            m_BaseUrl = baseUri.ToString();
            m_Username = username;
            m_Password = password;
            m_Currency = currency;
            m_CommissionRate = commissionRate;

            _SetupHttp(httpClient);

            if (object.ReferenceEquals(httpClient, httpClientLongTimeout) == false)
            {
                _SetupHttp(httpClientLongTimeout);
            }
        }

        public string Currency
        {
            get
            {
                return m_Currency;
            }
        }

        public PriceFormat PriceFormat
        {
            get
            {
                return m_PriceFormat;
            }
        }

        public decimal CommissionRate
        {
            get
            {
                return m_CommissionRate;
            }
        }

        private static void _SetupHttp(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue();
            httpClient.DefaultRequestHeaders.CacheControl.MaxAge = TimeSpan.Zero;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36");
        }

        public async Task<LoginResponse> Login(CancellationToken cancellation = default)
        {
            string loginUrl = $"{m_BaseUrl}user/login";

            var loginRequest = new LoginRequest
            {
                username = m_Username,
                password = m_Password,
            };

            LoginResponse loginResponse = await m_HttpClientHelper.PostAsync<LoginResponse, LoginRequest>(m_HttpClient, loginUrl, loginRequest, cancellation: cancellation).ConfigureAwait(false);
            m_Session = loginResponse?.data?.user?.auth;

            if (loginResponse?.data?.user?.oddsFormat != "american")
            {
                m_PriceFormat = PriceFormat.Decimal;
            }

            return loginResponse;
        }

        private async Task _KillSockets()
        {
            try
            {
                if (m_UserSocket is object)
                {
                    m_UserSocket.OnConnected -= _UserSocket_OnConnected;
                    m_UserSocket.OnReconnected -= _UserSocket_OnReconnected;
                    m_UserSocket.OnDisconnected -= _UserSocket_OnDisconnected;
                    m_UserSocket.OnError -= _UserSocket_OnError;

                    if (m_UserSocket.Connected == true)
                    {
                        await m_UserSocket.DisconnectAsync();
                    }

                    m_UserSocket.Dispose();
                }
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error cleaning up user websocket");
            }

            m_UserSocket = null;

            try
            {
                if (m_PublicSocket is object)
                {
                    m_PublicSocket.OnConnected -= _PublicSocket_OnConnected;
                    m_PublicSocket.OnReconnected -= _PublicSocket_OnReconnected;
                    m_PublicSocket.OnDisconnected -= _PublicSocket_OnDisconnected;
                    m_PublicSocket.OnError -= _PublicSocket_OnError;

                    if (m_PublicSocket.Connected == true)
                    {
                        await m_PublicSocket.DisconnectAsync();
                    }

                    m_PublicSocket.Dispose();
                }
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error cleaning up public websocket");
            }

            m_PublicSocket = null;
        }

        public async Task InitialiseWebSockets()
        {
            await _KillSockets();

            var options = new SocketIOOptions
            {
                Reconnection = true,
                Transport = TransportProtocol.WebSocket,
                ExtraHeaders = new(),
            };

            options.ExtraHeaders.Add("authorization", m_Session);
            string address = $"{m_SocketUri}v2/user/{m_Username}";

            m_UserSocket = new SocketIOClient.SocketIO(address, options);
            m_UserSocket.OnAny(_UserSocketMessageReceived);

            m_UserSocket.OnConnected += _UserSocket_OnConnected;
            m_UserSocket.OnReconnected += _UserSocket_OnReconnected;
            m_UserSocket.OnDisconnected += _UserSocket_OnDisconnected;
            m_UserSocket.OnError += _UserSocket_OnError;

            var publicOptions = new SocketIOOptions
            {
                Reconnection = true,
                Transport = TransportProtocol.WebSocket,
                ExtraHeaders = new(),
            };

            publicOptions.ExtraHeaders.Add("authorization", m_Session);
            string publicAddress = $"{m_SocketUri}priceUpdates";

            m_PublicSocket = new SocketIOClient.SocketIO(publicAddress, publicOptions);
            m_PublicSocket.OnAny(_PublicSocketMessageReceived);

            m_PublicSocket.OnConnected += _PublicSocket_OnConnected;
            m_PublicSocket.OnReconnected += _PublicSocket_OnReconnected;
            m_PublicSocket.OnDisconnected += _PublicSocket_OnDisconnected;
            m_PublicSocket.OnError += _PublicSocket_OnError;

            await m_UserSocket.ConnectAsync();
            await m_PublicSocket.ConnectAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _KillSockets();
        }

        private void _UserSocket_OnError(object sender, string e)
        {
            m_Logger.LogError("UserSocketError: " + e);
        }

        private void _UserSocket_OnConnected(object sender, EventArgs e)
        {
            UserWebSocketConnected?.Invoke(this, EventArgs.Empty);
        }

        private void _UserSocket_OnReconnected(object sender, int e)
        {
            UserWebSocketConnected?.Invoke(this, EventArgs.Empty);
        }

        private void _UserSocket_OnDisconnected(object sender, string e)
        {
            m_Logger.LogDebug($"WebSocket disconnected because {e}");
            UserWebSocketDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void _PublicSocket_OnError(object sender, string e)
        {
            m_Logger.LogError("PublicSocketError: " + e);
        }

        private void _PublicSocket_OnConnected(object sender, EventArgs e)
        {
            PublicWebSocketConnected?.Invoke(this, EventArgs.Empty);
        }

        private void _PublicSocket_OnReconnected(object sender, int e)
        {
            PublicWebSocketConnected?.Invoke(this, EventArgs.Empty);
        }

        private void _PublicSocket_OnDisconnected(object sender, string e)
        {
            PublicWebSocketDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private Dictionary<string, string> _CreateAuthHeader()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", m_Session }
            };
            return headers;
        }

        public async Task<ParticipantResponse> GetParticipants(CancellationToken cancellation = default)
        {
            string url = $"{m_BaseUrl}exchange/getParticipants";
            return await m_HttpClientHelper.GetAsync<ParticipantResponse>(m_HttpClient, url, additionalHeaders: _CreateAuthHeader(), requestName: "Participants", cancellation: cancellation).ConfigureAwait(false);
        }

        public async Task<LeaguesResponse> GetLeagues(CancellationToken cancellation = default)
        {
            string url = $"{m_BaseUrl}exchange/getLeagues";
            return await m_HttpClientHelper.GetAsync<LeaguesResponse>(m_HttpClient, url, additionalHeaders: _CreateAuthHeader(), requestName: "Leagues", cancellation: cancellation).ConfigureAwait(false);
        }

        public async Task<GamesResponse> GetGames(string league = "upcoming", string sport = null, CancellationToken cancellation = default)
        {
            string url = $"{m_BaseUrl}exchange/v2/getGames?league={league}";

            if (string.IsNullOrWhiteSpace(sport) == false)
            {
                url += $"&sport={sport}";
            }

            GamesResponse response = await m_HttpClientHelper.GetAsync<GamesResponse>(m_HttpClientLongTimeout, url, additionalHeaders: _CreateAuthHeader(), requestName: $"Games_{league}", cancellation: cancellation).ConfigureAwait(false);

            if (response?.data?.games?.Length > 0)
            {
                response.data.games.ForEach(g => g.SetProperties(m_PriceFormat, m_Currency));
            }

            return response;
        }

        public async Task<OrderBookResponse> GetOrderBook(string league = "upcoming", CancellationToken cancellation = default)
        {
            string url = $"{m_BaseUrl}exchange/v2/getOrderbook";

            var request = new OrderBookRequest
            {
                token = m_Session,
                league = league,
            };

            OrderBookResponse response = await m_HttpClientHelper.PostAsync<OrderBookResponse, OrderBookRequest>(m_HttpClient, url, request, additionalHeaders: _CreateAuthHeader(), requestName: $"OrderBook_{league}", cancellation: cancellation).ConfigureAwait(false);

            if (response?.data?.games?.Length > 0)
            {
                response.data.games.ForEach(g => g.SetProperties(m_PriceFormat, m_Currency));
            }

            return response;
        }

        public async Task<MatchedBetsResponse> GetMatchedBets(CancellationToken cancellation = default)
        {
            string url = $"{m_BaseUrl}user/getMatchedBets";

            MatchedBetsResponse response = await m_HttpClientHelper.GetAsync<MatchedBetsResponse>(m_HttpClientLongTimeout, url, additionalHeaders: _CreateAuthHeader(), requestName: $"MatchedBets", cancellation: cancellation).ConfigureAwait(false);

            if (response?.data?.matchedBets?.Length > 0)
            {
                response.data.matchedBets.ForEach(m => m.SetProperties(m_PriceFormat, m_Currency, m_CommissionRate));
            }

            return response;
        }

        public async Task<UnmatchedBetsResponse> GetUnmatchedBets(CancellationToken cancellation = default)
        {
            string url = $"{m_BaseUrl}user/getUnmatched";

            UnmatchedBetsResponse response = await m_HttpClientHelper.GetAsync<UnmatchedBetsResponse>(m_HttpClientLongTimeout, url, additionalHeaders: _CreateAuthHeader(), requestName: $"UnmatchedBets", cancellation: cancellation).ConfigureAwait(false);

            if (response?.data?.unmatched?.Length > 0)
            {
                response.data.unmatched.ForEach(u => u.SetProperties(m_PriceFormat, m_Currency, m_CommissionRate));
            }

            return response;
        }

        public async Task<GradedWagersResponse> GetGradedWagers(DateTime startDate, DateTime endDate, CancellationToken cancellation = default)
        {
            string url = $"{m_BaseUrl}user/getGradedWagers?startDate={startDate:MM-dd-yyyy}&endDate={endDate:MM-dd-yyyy}";

            GradedWagersResponse response = await m_HttpClientHelper.GetAsync<GradedWagersResponse>(m_HttpClientLongTimeout, url, additionalHeaders: _CreateAuthHeader(), requestName: $"GradedWagers_{startDate:MM-dd-yyyy}_{endDate:MM-dd-yyyy}", cancellation: cancellation).ConfigureAwait(false);

            if (response?.data?.graded?.Length > 0)
            {
                response.data.graded.ForEach(g => g.SetProperties(m_PriceFormat, m_Currency, m_CommissionRate));
            }

            return response;
        }

        private void _UserSocketMessageReceived(string eventName, SocketIOResponse message)
        {
            string data = message.GetValue<string>();
            m_Logger.LogDebug(data);

            if (eventName == "positionUpdate")
            {
                PositionUpdateMessage positionUpdateMessage = JsonConvert.DeserializeObject<PositionUpdateMessage>(data);
                positionUpdateMessage.SetProperties(m_PriceFormat, m_Currency, m_CommissionRate);
                PositionUpdated?.Invoke(this, positionUpdateMessage);
            }
        }

        private void _PublicSocketMessageReceived(string eventName, SocketIOResponse message)
        {
            string data = message.GetValue<string>();

            if (eventName == "gameUpdate")
            {
                m_Logger.LogDebug(data);

                GameUpdateMessage gameUpdateMessage = JsonConvert.DeserializeObject<GameUpdateMessage>(data);
                GameUpdated?.Invoke(this, gameUpdateMessage);
                return;
            }
            if (eventName == "orderUpdate")
            {
                OrderUpdateMessage orderUpdateMessage = JsonConvert.DeserializeObject<OrderUpdateMessage>(data);
                OrderUpdated?.Invoke(this, orderUpdateMessage);
                return;
            }
        }

        public async Task<PlaceResponse> Place(IEnumerable<PlaceOrder> orders, CancellationToken cancellation = default)
        {
            string url = $"{m_BaseUrl}session/v2/place";

            var request = new PlaceRequest
            {
                token = m_Session,
                orders = orders.ToArray(),
            };

            PlaceResponse response = await m_HttpClientHelper.PostAsync<PlaceResponse, PlaceRequest>(m_HttpClient, url, request, additionalHeaders: _CreateAuthHeader(), requestName: $"PlaceOrders", cancellation: cancellation).ConfigureAwait(false);
            response?.SetProperties(m_PriceFormat, m_Currency, m_CommissionRate);
            return response;
        }

        public async Task<CancelAllOrdersResponse> CancelAllOrders(CancellationToken cancellation = default)
        {
            string url = $"{m_BaseUrl}session/cancelAllOrders";

            var request = new CancelAllOrdersRequest
            {
                token = m_Session,
            };

            CancelAllOrdersResponse response = await m_HttpClientHelper.PostAsync<CancelAllOrdersResponse, CancelAllOrdersRequest>(m_HttpClientLongTimeout, url, request, additionalHeaders: _CreateAuthHeader(), requestName: $"CancelAllOrders", cancellation: cancellation).ConfigureAwait(false);
            return response;
        }

        public async Task<CancelResponse> Cancel(string sessionId, CancellationToken cancellation = default)
        {
            string url = $"{m_BaseUrl}session/cancel";

            var request = new CancelRequest
            {
                token = m_Session,
                sessionID = sessionId,
            };

            CancelResponse response = await m_HttpClientHelper.PostAsync<CancelResponse, CancelRequest>(m_HttpClient, url, request, additionalHeaders: _CreateAuthHeader(), requestName: $"Cancel", cancellation: cancellation).ConfigureAwait(false);
            return response;
        }

        public async Task<CancelMultipleResponse> CancelMultiple(IEnumerable<string> sessionIds, CancellationToken cancellation = default)
        {
            string url = $"{m_BaseUrl}session/cancelMultiple";

            var request = new CancelMultipleRequest
            {
                token = m_Session,
                sessionIDs = sessionIds.ToArray(),
            };

            CancelMultipleResponse response = await m_HttpClientHelper.PostAsync<CancelMultipleResponse, CancelMultipleRequest>(m_HttpClient, url, request, additionalHeaders: _CreateAuthHeader(), requestName: $"CancelMultiple", cancellation: cancellation).ConfigureAwait(false);
            return response;
        }

        //public async Task<EditOrderResponse> EditOrder(Int32 stakeUSD, string gameId, Price price, CancellationToken cancellation = default)
        //{
        //    string url = $"{m_BaseUrl}session/editOrder";

        //    var request = new EditOrderRequest
        //    {
        //        token = m_Session,
        //    };

        //    EditOrderResponse response = await m_HttpClientHelper.PostAsync<EditOrderResponse, EditOrderRequest>(m_HttpClient, url, request, cancellation: cancellation).ConfigureAwait(false);
        //    return response;
        //}
    }
}
