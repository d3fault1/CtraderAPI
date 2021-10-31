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
        public int ClosingTicket { get; set; }
        public string Symbol { get; set; }
        public string Type { get; set; }
        public double OpenPrice { get; set; }
        public DateTime OpenTime { get; set; }
        public double ClosePrice { get; set; }
        public DateTime CloseTime { get; set; }
        public double Profit { get; set; }
        public double Volume { get; set; }
        public string Comment { get; set; }
        public double Commission { get; set; }
        public double Swap { get; set; }
        public double StopLoss { get; set; }
        public double TakeProfit { get; set; }
    }

    public struct CtQuote : ICtQuote
    {
        public string Symbol { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
    }

    public struct CtOrder : ICtOrderData
    {
        public int Ticket { get; set; }
        public int ClosingTicket { get; set; }
        public string Symbol { get; set; }
        public string Type { get; set; }
        public double OpenPrice { get; set; }
        public DateTime OpenTime { get; set; }
        public double ClosePrice { get; set; }
        public DateTime CloseTime { get; set; }
        public double Profit { get; set; }
        public double Volume { get; set; }
        public string Comment { get; set; }
        public double Commission { get; set; }
        public double Swap { get; set; }
        public double StopLoss { get; set; }
        public double TakeProfit { get; set; }
    }

    public struct ConnectionStateEventArgs
    {
        public CtConnectionState State { get; }
        public string Message { get; }

        public ConnectionStateEventArgs(CtConnectionState state, string message)
        {
            State = state;
            Message = message;
        }
    }

    public enum CtConnectionState
    {
        Connected,
        Connecting,
        Disconnected,
        Failed
    }
}
