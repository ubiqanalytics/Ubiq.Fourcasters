using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Ubiq.Extensions.Newtonsoft;
using Ubiq.FourcastersAPI;
using Ubiq.Http;

HttpClient httpClient = HttpUtil.CreateHttpClient(false, false, false);
var httpClientHelper = new HttpClientHelper(NullLogger<HttpClientHelper>.Instance, new JsonSerializerSettings().Configure());

string username = "your username";
string password = "your password";

var fourcasters = new FourcastersAPI(NullLogger<FourcastersAPI>.Instance, httpClientHelper, httpClient, new Uri("https://api.4casters.io/"), username, password, "USD", 1.0m);

fourcasters.OrdersUpdated += Fourcasters_OrdersUpdated;
fourcasters.PositionUpdated += Fourcasters_PositionUpdated;

void Fourcasters_PositionUpdated(object sender, PositionUpdateMessage[] e)
{
    Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
}

void Fourcasters_OrdersUpdated(object sender, OrderUpdateMessage[] e)
{
    Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
}

await fourcasters.Login();
await fourcasters.InitialiseWebSockets();

CancelAllOrdersResponse cancelAllResponse = await fourcasters.CancelAllOrders();

GamesResponse games = await fourcasters.GetGames("NHL");
Game game = games.data.games.First();

PlaceResponse placeResponse = await fourcasters.Place(new[] { new PlaceOrder()
    {
        gameID = game.id,
        odds = +500m,
        bet = 10m,
        side = game.participants.First().id,
        type = "moneyline",
    }
});

CancelResponse cancelResponse = await fourcasters.Cancel(placeResponse.data.createdSessions.First().unmatched.OfferId);

Console.ReadLine();
