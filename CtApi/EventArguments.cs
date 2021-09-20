using System;
using CTApiService;

namespace CTApi
{
    public struct CtQuoteEventArgs : ICtQuote
    {
        public string Symbol { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
    }

    public struct CtOrderDataEventArgs : ICtOrderData
    {
        public int Ticket { get; set; }
        public string Symbol { get; set; }
        public string Type { get; set; }
        public double OpenPrice { get; set; }
        public DateTime OpenTime { get; set; }
        public double ClosePrice { get; set; }
        public DateTime CloseTime { get; set; }
        public double Profit { get; set; }
        public double Volume { get; set; }
        public string Comment { get; set; }
        public double Commision { get; set; }
        public double Swap { get; set; }
        public double StopLoss { get; set; }
        public double TakeProfit { get; set; }
    }
}
