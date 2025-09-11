using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Ubiq.Extensions.Newtonsoft;
using Ubiq.FourcastersAPI;
using Ubiq.Http;

HttpClient httpClient = HttpUtil.CreateHttpClient(false, false, false);
var httpClientHelper = new HttpClientHelper(NullLogger<HttpClientHelper>.Instance, new JsonSerializerSettings().Configure());

string username = "";
string password = "";

var fourcasters = new FourcastersAPI(NullLogger<FourcastersAPI>.Instance, httpClientHelper, httpClient, httpClient, new Uri("https://api.4casters.io/"), new Uri("wss://socket-api.4casters.io"), username, password, "USD", 1.0m);

fourcasters.OrderUpdated += Fourcasters_OrdersUpdated;
fourcasters.PositionUpdated += Fourcasters_PositionUpdated;

void Fourcasters_PositionUpdated(object sender, PositionUpdateMessage e)
{
    Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
}

void Fourcasters_OrdersUpdated(object sender, OrderUpdateMessage e)
{
    Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
}

await fourcasters.Login();
await fourcasters.InitialiseWebSockets();

LeaguesResponse leagues = await fourcasters.GetLeagues();

CancelAllOrdersResponse cancelAllResponse = await fourcasters.CancelAllOrders();

GamesResponse games = await fourcasters.GetGames("MLB");
Game game = games.data.games.First();

PlaceResponse placeResponse1 = await fourcasters.Place([new PlaceOrder
    {
        gameID = game.id,
        odds = +500m,
        bet = 10m,
        side = game.participants.First().id,
        type = "moneyline",
    }
]);
PlaceResponse placeResponse2 = await fourcasters.Place([new PlaceOrder
    {
        gameID = game.id,
        odds = +500m,
        bet = 10m,
        side = game.participants.Last().id,
        type = "moneyline",
    }
]);

CancelResponse cancel1 = await fourcasters.Cancel(placeResponse1.data.createdSessions.First().unmatched.OfferId);
CancelResponse cancel2 = await fourcasters.Cancel(placeResponse1.data.createdSessions.First().unmatched.OfferId);

CancelMultipleResponse cancelResponse1 = await fourcasters.CancelMultiple([placeResponse1.data.createdSessions.First().unmatched.OfferId, placeResponse2.data.createdSessions.First().unmatched.OfferId]);
CancelMultipleResponse cancelResponse2 = await fourcasters.CancelMultiple([placeResponse1.data.createdSessions.First().unmatched.OfferId, placeResponse2.data.createdSessions.First().unmatched.OfferId]);

Console.ReadLine();
