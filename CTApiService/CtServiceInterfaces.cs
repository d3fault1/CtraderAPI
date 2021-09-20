using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace CTApiService
{
    [ServiceContract(CallbackContract = typeof(ICtApiCallback), SessionMode = SessionMode.Required)]
    public interface ICtApi
    {
        [OperationContract(IsInitiating = true)]
        bool Connect();
        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect();
        [OperationContract]
        string AccountCompany();
        [OperationContract]
        string AccountServer();
        [OperationContract]
        string AccountName();
        [OperationContract]
        int AccountNumber();
        [OperationContract]
        int AccountLevarage();
        [OperationContract]
        string AccountCurrency();
        [OperationContract]
        DateTime ServerTime();
        [OperationContract]
        double AccountBalance();
        [OperationContract]
        double AccountEquity();
        [OperationContract]
        List<CtOrderData> AccountHistory(DateTime from, DateTime to);
        [OperationContract]
        List<CtOrderData> AccountPositions();
        [OperationContract]
        bool CloseAllPositions();
        [OperationContract]
        bool ClosePosition(int ticket);
    }

    [ServiceContract]
    public interface ICtApiCallback
    {
        [OperationContract(IsOneWay = true)]
        void QuoteOccured(CtQuote quote);
        [OperationContract(IsOneWay = true)]
        void PositionOpenOccured(CtOrderData position);
        [OperationContract(IsOneWay = true)]
        void PositionCloseOccured(CtOrderData position);
        [OperationContract(IsOneWay = true)]
        void PositionModifiedOccured(CtOrderData position);
    }

    public interface ICtOrderData
    {
        int Ticket { get; set; }
        string Symbol { get; set; }
        string Type { get; set; }
        double OpenPrice { get; set; }
        DateTime OpenTime { get; set; }
        double ClosePrice { get; set; }
        DateTime CloseTime { get; set; }
        double Profit { get; set; }
        double Volume { get; set; }
        string Comment { get; set; }
        double Commision { get; set; }
        double Swap { get; set; }
        double StopLoss { get; set; }
        double TakeProfit { get; set; }
    }

    public interface ICtQuote
    {
        string Symbol { get; set; }
        double Bid { get; set; }
        double Ask { get; set; }
    }

    [DataContract]
    public class CtOrderData : ICtOrderData
    {
        [DataMember]
        public int Ticket { get; set; }
        [DataMember]
        public string Symbol { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public double OpenPrice { get; set; }
        [DataMember]
        public DateTime OpenTime { get; set; }
        [DataMember]
        public double ClosePrice { get; set; }
        [DataMember]
        public DateTime CloseTime { get; set; }
        [DataMember]
        public double Profit { get; set; }
        [DataMember]
        public double Volume { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public double Commision { get; set; }
        [DataMember]
        public double Swap { get; set; }
        [DataMember]
        public double StopLoss { get; set; }
        [DataMember]
        public double TakeProfit { get; set; }
    }

    [DataContract]
    public class CtQuote : ICtQuote
    {
        [DataMember]
        public string Symbol { get; set; }
        [DataMember]
        public double Bid { get; set; }
        [DataMember]
        public double Ask { get; set; }
    }
}
