using Newtonsoft.Json;
using System.Runtime.Serialization;
using Ubiq.Definitions.Market;
using Ubiq.Extensions;

namespace Ubiq.FourcastersAPI
{
    public class LoginRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class LoginResponse
    {
        public LoginData data { get; set; }
    }

    public class LoginData
    {
        public User user { get; set; }
    }

    public class User
    {
        public bool seedAccess { get; set; }
        public bool isAdmin { get; set; }
        public string id { get; set; }
        public string username { get; set; }
        public decimal displayBalance { get; set; }
        public decimal matchedBalance { get; set; }
        public decimal liability { get; set; }
        public decimal creditLimit { get; set; }
        public string language { get; set; }
        public string auth { get; set; }
        public string oddsFormat { get; set; }
        public string type { get; set; }
        public bool hasMarketMakerAccess { get; set; }
        public bool sportsbookDefault { get; set; }
        public bool defaultFollowPinny { get; set; }
        public bool defaultPinnyThruNumber { get; set; }
        public string accessCode { get; set; }
        public decimal commissionCharged { get; set; }
        public bool isPro { get; set; }
    }

    public class AuthenticatedRequest
    {
        public string token { get; set; }
    }

    public class ParticipantRequest : AuthenticatedRequest
    {
    }

    public class ParticipantResponse
    {
        public ParticipantData data { get; set; }
    }

    public class ParticipantData
    {
        public Participant[] participants { get; set; }
    }

    public class Participant
    {
        public string participantID { get; set; }
        public string longName { get; set; }
        public string shortName { get; set; }
        public string league { get; set; }
        public string sport { get; set; }
    }

    public class GamesRequest : AuthenticatedRequest
    {
        public string league { get; set; }
    }

    public class GamesResponse
    {
        public GamesData data { get; set; }
    }

    public class GamesData
    {
        public Game[] games { get; set; }
    }

    public class Game
    {
        public bool isFutures { get; set; }
        public string eventName { get; set; }
        public string futuresTeam { get; set; }
        public string id { get; set; }
        public string parentGameID { get; set; }
        public string cheapDataUID { get; set; }
        public string league { get; set; }
        public string sport { get; set; }
        public DateTime start { get; set; }
        public bool ended { get; set; }
        public bool featured { get; set; }
        public bool live { get; set; }
        public GameParticipant[] participants { get; set; }
        public Order[] awayMoneylines { get; set; }
        public Order[] homeMoneylines { get; set; }
        public Order[] awaySpreads { get; set; }
        public Order[] homeSpreads { get; set; }
        public Order[] over { get; set; }
        public Order[] under { get; set; }

        public void SetProperties(PriceFormat priceFormat, string currency)
        {
            _SetOrderProperties(priceFormat, currency, awayMoneylines);
            _SetOrderProperties(priceFormat, currency, homeMoneylines);
            _SetOrderProperties(priceFormat, currency, awaySpreads);
            _SetOrderProperties(priceFormat, currency, homeSpreads);
            _SetOrderProperties(priceFormat, currency, over);
            _SetOrderProperties(priceFormat, currency, under);
        }

        private void _SetOrderProperties(PriceFormat priceFormat, string currency, Order[] orders)
        {
            if (orders?.Length > 0)
            {
                orders.ForEach(o => o.SetProperties(priceFormat, currency));
            }
        }
    }

    public class GameParticipant
    {
        public string id { get; set; }
        public string longName { get; set; }
        public string shortName { get; set; }
        public string mainPitcher { get; set; }
        public string homeAway { get; set; }
        public string rotationNumber { get; set; }
        public string futuresSide { get; set; }
        public Int32? score { get; set; }
    }

    public class GameUpdateMessage
    {
        public bool isFutures { get; set; }
        public string futuresTeam { get; set; }
        public string id { get; set; }
        public string parentGameID { get; set; }
        public string cheapDataUID { get; set; }
        public string league { get; set; }
        public string sport { get; set; }
        public DateTime start { get; set; }
        public bool ended { get; set; }
        public GameParticipant[] participants { get; set; }
        public string eventName { get; set; }
        public decimal? mainHomeSpread { get; set; }
        public decimal? mainAwaySpread { get; set; }
        public decimal? mainTotal { get; set; }
    }

    public class OrderBookRequest : AuthenticatedRequest
    {
        public string league { get; set; }
    }

    public class OrderBookResponse
    {
        public OrderBookData data { get; set; }
    }

    public class OrderBookData
    {
        public string market { get; set; }
        public Game[] games { get; set; }
    }

    public class Order
    {
        public string id { get; set; }
        public string type { get; set; }
        public string createdBy { get; set; }
        public decimal sumUntaken { get; set; }
        public decimal odds { get; set; }
        public decimal bet { get; set; }
        public string gameID { get; set; }
        public decimal takenRatio { get; set; }
        public string participantID { get; set; }
        public decimal? total { get; set; }
        public decimal? spread { get; set; }
        public string OU { get; set; }
        public string source { get; set; }
        public int? level { get; set; }
        public DateTime? expiry { get; set; }
        public DateTime createdAt { get; set; }
        //public bool followPinnacle { get; set; }
        //public int pinnacleOdds { get; set; }

        public Price Price { get; set; }
        public Amount Amount { get; set; }

        public void SetProperties(PriceFormat priceFormat, string currency)
        {
            this.Price = new Price(priceFormat, odds);
            this.Amount = new Amount(bet, currency);
        }
    }

    public class MatchedBetsResponse
    {
        public MatchedBetData data { get; set; }
    }

    public class MatchedBetData
    {
        public Bet[] matchedBets { get; set; }
    }

    public class UnmatchedBetsResponse
    {
        public UnmatchedBetData data { get; set; }
    }

    public class UnmatchedBetData
    {
        public Bet[] unmatched { get; set; }
    }

    public class GradedWagersResponse
    {
        public GradedWagersData data { get; set; }
    }

    public class GradedWagersData
    {
        public Bet[] graded { get; set; }
    }

    public class Pagination
    {
        public int totalDocs { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
        public int totalPages { get; set; }
        public int page { get; set; }
        public int pagingCounter { get; set; }
        public bool hasPrevPage { get; set; }
        public bool hasNextPage { get; set; }
        public int? prevPage { get; set; }
        public int? nextPage { get; set; }
    }

    public class Ordersummary
    {
        public Moneyline ml { get; set; }
        public Spread spread { get; set; }
        public Total total { get; set; }
    }

    public class Moneyline
    {
        public decimal homeRisk { get; set; }
        public decimal awayRisk { get; set; }
        public decimal homeWin { get; set; }
        public decimal awayWin { get; set; }
    }

    public class Spread
    {
        public decimal homeRisk { get; set; }
        public decimal awayRisk { get; set; }
        public decimal homeWin { get; set; }
        public decimal awayWin { get; set; }
    }

    public class Total
    {
        public decimal overRisk { get; set; }
        public decimal underRisk { get; set; }
        public decimal overWin { get; set; }
        public decimal underWin { get; set; }
    }

    public class OrderUpdateMessage
    {
        public string gameID { get; set; }
        public string sport { get; set; }
        public string type { get; set; }
        public bool live { get; set; }
        public Sideorder[] sideOrders { get; set; }
        public string participantID { get; set; }
    }

    public class Sideorder
    {
        public string id { get; set; }
        public string type { get; set; }
        public string createdBy { get; set; }
        public decimal sumUntaken { get; set; }
        public decimal odds { get; set; }
        public decimal bet { get; set; }
        public string gameID { get; set; }
        public decimal? takenRatio { get; set; }
        public string participantID { get; set; }
        public decimal? total { get; set; }
        public decimal? spread { get; set; }
        public string OU { get; set; }
        public bool followPinnacle { get; set; }
        public decimal pinnacleOdds { get; set; }
        public string source { get; set; }
        public int? level { get; set; }
        public DateTime? expiry { get; set; }
        public DateTime createdAt { get; set; }
        public bool gameStartExpiry { get; set; }
    }

    public interface IBet
    {
        string OfferId { get; }
        string BetId { get; }
        decimal PriceDecimalWithCommission { get; }
        decimal CommissionRate { get; }
        bool IsOffer { get; }
        Price Price { get; }
        Amount Risk { get; }
        Amount Win { get; }

        void SetProperties(PriceFormat priceFormat, string currency, decimal commissionRate);
    }

    public class Bet : IBet
    {
        [NonSerialized]
        [JsonIgnore]
        [IgnoreDataMember]
        private decimal m_CommissionRate;

        public Game game { get; set; }
        public string id { get; set; }
        public bool graded { get; set; }
        public string type { get; set; }
        public decimal bet { get; set; }
        public string txID { get; set; }
        public bool closed { get; set; }
        public DateTime createdAt { get; set; }
        public string origin { get; set; }
        public bool adminRefund { get; set; }
        public string ticketNumber { get; set; }
        public bool? cancelled { get; set; }
        public decimal odds { get; set; }
        public decimal? spread { get; set; }
        public decimal? total { get; set; }
        public string participantID { get; set; }
        public DateTime? matchedTime { get; set; }
        public string OU { get; set; }
        public string outcome { get; set; }
        public DateTime? expiry { get; set; }
        public decimal? takenRatio { get; set; }
        public string otherParticipantID { get; set; }
        public string risk { get; set; }
        public string win { get; set; }
        public string fee { get; set; }
        public string result { get; set; }
        public DateTime? settledAt { get; set; }
        public string platform { get; set; }
        //public Pinnacleline latestPinnacleLine { get; set; }
        //public Pinnacleline pinnacleLine { get; set; }

        public string OfferId
        {
            get
            {
                return this.id;
            }
        }

        public string BetId
        {
            get
            {
                return this.txID;
            }
        }

        public decimal PriceDecimalWithCommission
        {
            get
            {
                return (this.Win.Value / this.Risk.Value) + 1m;
            }
        }

        public decimal CommissionRate
        {
            get
            {
                if (this.IsOffer == false)
                {
                    return m_CommissionRate;
                }

                return 0m;
            }
        }

        public bool IsOffer
        {
            get
            {
                return this.origin == "offer";
            }
        }

        public Price Price { get; private set; }
        public Amount Risk { get; private set; }
        public Amount Win { get; private set; }
        public Amount Result { get; private set; }

        public void SetProperties(PriceFormat priceFormat, string currency, decimal commissionRate)
        {
            game?.SetProperties(priceFormat, currency);

            m_CommissionRate = commissionRate;
            this.Price = new Price(priceFormat, odds);

            if (string.IsNullOrWhiteSpace(this.risk) == false)
            {
                this.Risk = new Amount(decimal.Parse(this.risk), currency);
            }

            if (string.IsNullOrWhiteSpace(this.win) == false)
            {
                this.Win = new Amount(decimal.Parse(this.win), currency);
            }

            if (string.IsNullOrWhiteSpace(result) == false)
            {
                decimal pnl = decimal.Parse(result);
                this.Result = new Amount(pnl, currency);
            }
        }
    }

    // BET PLACEMENT /////////////////////////////////////////////////////////////////////////////////

    public class PlaceRequest : AuthenticatedRequest
    {
        public PlaceOrder[] orders { get; set; }
    }

    public class PlaceOrder
    {
        public string gameID { get; set; }
        public decimal odds { get; set; }
        public decimal bet { get; set; }
        public string type { get; set; }
        public decimal? number { get; set; }
        public string side { get; set; }
        public string userReference { get; set; }

        public override string ToString()
        {
            return $"({this.gameID}, {this.type}, {this.number}, {this.odds}, {this.bet})";
        }
    }

    public class PlaceResponse
    {
        public PlaceData data { get; set; }

        public void SetProperties(PriceFormat priceFormat, string currency, decimal commissionRate)
        {
            if (this.data?.createdSessions?.Length > 0)
            {
                foreach (PlacedSession session in this.data.createdSessions)
                {
                    if (session is null)
                    {
                        continue;
                    }

                    if (session.matched?.Length > 0)
                    {
                        session.matched.ForEach(s => s.SetProperties(priceFormat, currency, commissionRate));
                    }

                    if (session.unmatched is object)
                    {
                        session.unmatched.SetProperties(currency);
                    }
                }
            }
        }
    }

    public class PlaceData
    {
        public PlacedSession[] createdSessions { get; set; }
    }

    public class PlacedSession
    {
        public string error { get; set; }
        public PlacedMatched[] matched { get; set; }
        public PlacedUnmatched unmatched { get; set; }
    }

    public class PlacedMatched : IBet
    {
        [NonSerialized]
        [JsonIgnore]
        [IgnoreDataMember]
        private decimal m_CommissionRate;

        public string txID { get; set; }
        public string orderID { get; set; }
        public decimal risk { get; set; }
        public decimal win { get; set; }
        public decimal odds { get; set; }
        public decimal amount { get; set; }
        public string type { get; set; }
        public decimal? number { get; set; }
        public string side { get; set; }
        public string wagerRequestID { get; set; }
        public string userReference { get; set; }

        public Price Price { get; private set; }
        public Amount Risk { get; private set; }
        public Amount Win { get; private set; }

        public string OfferId
        {
            get
            {
                return this.orderID;
            }
        }

        public string BetId
        {
            get
            {
                return this.txID;
            }
        }

        public decimal PriceDecimalWithCommission
        {
            get
            {
                return (this.Win.Value / this.Risk.Value) + 1m;
            }
        }

        public decimal CommissionRate
        {
            get
            {
                // commission set because instantly matched from /place = wager
                return m_CommissionRate;
            }
        }

        public bool IsOffer
        {
            get
            {
                // instantly matched from /place = wager
                return false;
            }
        }

        public void SetProperties(PriceFormat priceFormat, string currency, decimal commissionRate)
        {
            m_CommissionRate = commissionRate;
            this.Price = new Price(priceFormat, odds);
            this.Risk = new Amount(this.risk, currency);
            this.Win = new Amount(this.win, currency);
        }
    }

    public class PlacedUnmatched
    {
        public string orderID { get; set; }
        public decimal offered { get; set; }
        public decimal odds { get; set; }
        public string type { get; set; }
        public decimal? number { get; set; }
        public string side { get; set; }
        public string wagerRequestID { get; set; }
        public string userReference { get; set; }

        public string OfferId
        {
            get
            {
                return this.orderID;
            }
        }

        public Amount Offered { get; private set; }

        public void SetProperties(string currency)
        {
            this.Offered = new Amount(this.offered, currency);
        }
    }

    public class PositionUpdateMessage
    {
        public string gameID { get; set; }
        public string parentGameID { get; set; }
        public string eventName { get; set; }
        public string league { get; set; }
        public string sport { get; set; }
        public string platform { get; set; }
        public string origin { get; set; }
        public string awayRotationNumber { get; set; }
        public bool live { get; set; }
        public DateTime start { get; set; }

        public Matched matched { get; set; }
        public Unmatched unmatched { get; set; }

        public void SetProperties(PriceFormat priceFormat, string currency, decimal commissionRate)
        {
            if (matched is object)
            {
                matched.origin = this.origin;
                matched.SetProperties(priceFormat, currency, commissionRate);
            }

            unmatched?.SetProperties(priceFormat, currency);
        }
    }

    public class Matched : IBet
    {
        [NonSerialized]
        [JsonIgnore]
        [IgnoreDataMember]
        private decimal m_CommissionRate;

        public string txID { get; set; }
        public string orderID { get; set; }
        public decimal? odds { get; set; }
        public decimal? amount { get; set; }
        public decimal? risk { get; set; }
        public decimal? win { get; set; }
        public string type { get; set; }
        public string side { get; set; }
        public decimal? number { get; set; }
        public string origin { get; set; }
        public string userReference { get; set; }

        public Price Price { get; private set; }
        public Amount Risk { get; private set; }
        public Amount Win { get; private set; }

        public string OfferId
        {
            get
            {
                return this.orderID;
            }
        }

        public string BetId
        {
            get
            {
                return this.txID;
            }
        }

        public decimal PriceDecimalWithCommission
        {
            get
            {
                return (this.Win.Value / this.Risk.Value) + 1m;
            }
        }

        public decimal CommissionRate
        {
            get
            {
                if (this.IsOffer == false)
                {
                    return m_CommissionRate;
                }

                return 0m;
            }
        }

        public bool IsOffer
        {
            get
            {
                return this.origin == "offer";
            }
        }

        public void SetProperties(PriceFormat priceFormat, string currency, decimal commissionRate)
        {
            m_CommissionRate = commissionRate;

            if (odds != null)
            {
                this.Price = new Price(priceFormat, odds.Value);
            }

            if (risk != null)
            {
                this.Risk = new Amount(risk.Value, currency);
            }

            if (win != null)
            {
                this.Win = new Amount(win.Value, currency);
            }
        }
    }

    public class Unmatched
    {
        public string orderID { get; set; }
        public decimal? filled { get; set; }
        public decimal? offered { get; set; }
        public decimal? remaining { get; set; }
        public decimal? odds { get; set; }
        public string type { get; set; }
        public string side { get; set; }
        public decimal? number { get; set; }
        public string userReference { get; set; }
        public string wagerRequestID { get; set; }

        public Price Price { get; private set; }
        public Amount Filled { get; private set; }
        public Amount Offered { get; private set; }
        public Amount Remaining { get; private set; }

        public string OfferId
        {
            get
            {
                return this.orderID;
            }
        }

        public void SetProperties(PriceFormat priceFormat, string currency)
        {
            if (odds != null)
            {
                this.Price = new Price(priceFormat, odds.Value);
            }

            if (remaining != null)
            {
                this.Remaining = new Amount(remaining.Value, currency);
            }
            if (offered != null)
            {
                this.Offered = new Amount(offered.Value, currency);
            }
            if (filled != null)
            {
                this.Filled = new Amount(filled.Value, currency);
            }
        }
    }

    public class CancelRequest : AuthenticatedRequest
    {
        public string sessionID { get; set; }
    }

    public class CancelResponse
    {
        public CancelData data { get; set; }
    }

    public class CancelData
    {
        public CancelledOrder cancelledOrder { get; set; }
    }

    public class CancelledOrder
    {
        public string sideString { get; set; }
        public decimal? odds { get; set; }
        public decimal? volume { get; set; }
    }

    public class CancelMultipleRequest : AuthenticatedRequest
    {
        public string[] sessionIDs { get; set; }
    }

    public class CancelMultipleResponse
    {
        public CancelledMultipleData[] data { get; set; }
    }

    public class CancelledMultipleData
    {
        public bool success { get; set; }
        public string sessionID { get; set; }
        public string gameID { get; set; }
    }

    public class CancelledSession
    {
        public string _id { get; set; }
        public CancelledGameId gameID { get; set; }
    }

    public class CancelledGameId
    {
        public string _id { get; set; }
    }

    public class CancelAllOrdersRequest : AuthenticatedRequest
    {
    }

    public class CancelAllOrdersResponse
    {
        public CancelAllOrdersData[] data { get; set; }
    }

    public class CancelAllOrdersData
    {
        public bool success { get; set; }
        public string sessionID { get; set; }
        public int odds { get; set; }
        public string gameID { get; set; }
    }


    //public class EditOrderRequest : AuthenticatedRequest
    //{
    //    public string sessionID { get; set; }
    //    public int orderOdds { get; set; }
    //    public int orderVolume { get; set; }
    //    public int expiryChange { get; set; }
    //    public bool followPinnacle { get; set; }
    //}

    //public class EditOrderResponse
    //{
    //    public EditOrderData data { get; set; }
    //}

    //public class EditOrderData
    //{
    //    public int balanceChange { get; set; }
    //    public int newOrderOdds { get; set; }
    //}
}
