using System;
using CTApiService;

namespace CTApi
{
    public class CtApi
    {
        private CtClient Client;
        private ConnectionState _state = ConnectionState.Disconnected;

        public delegate void ConnectionStateChangedEventHandler(object sender, ConnectionStateEventArgs e);
        public delegate void QuoteEventHandler(object sender, CtQuoteEventArgs e);
        public delegate void PositionOpenEventHandler(object sender, CtOrderDataEventArgs e);
        public delegate void PositionCloseEventHandler(object sender, CtOrderDataEventArgs e);
        public delegate void PositionModifiedEventHandler(object sender, CtOrderDataEventArgs e);

        public event ConnectionStateChangedEventHandler ConnectionStateChanged;
        public event QuoteEventHandler OnQuote;
        public event PositionOpenEventHandler OnPositionOpen;
        public event PositionCloseEventHandler OnPositionClose;
        public event PositionModifiedEventHandler OnPositionModify;

        public CtApi()
        {

        }

        public void Connect(int port)
        {
            if (_state == ConnectionState.Connected || _state == ConnectionState.Connecting)
            {
                return;
            }

            _state = ConnectionState.Connecting;
            Client = new CtClient(port);

            string message = $"Connecting to localhost:{port}";
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(_state, message));

            _state = ConnectionState.Failed;

            try
            {
                Client.Connect();
                _state = ConnectionState.Connected;
            }
            catch (Exception e)
            {
                Client.Dispose();
                message = $"Failed connection to localhost:{port}. Server not running.";
            }

            if (_state == ConnectionState.Connected)
            {
                //Client.ServiceStopped += ConnectionChangedCallback;
                Client.Interrupted += ConnectionChangedCallback;
                Client.OnQuote += QuoteUpdateCallback;
                Client.OnPositionOpen += PositionOpenCallback;
                Client.OnPositionModify += PositionModifyCallback;
                Client.OnPositionClose += PositionCloseCallback;

                message = $"Connected to localhost:{port}";
            }

            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(_state, message));
        }

        public void Disconnect()
        {
            _state = ConnectionState.Disconnected;
            var message = $"Server disconnected successfully";
            Client.Disconnect();
            Client.Dispose();
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(_state, message));
        }

        public double AccountBalance() => Client.AccountBalance();
        public string AccountCompany() => Client.AccountCompany();
        public string AccountCurrency() => Client.AccountCurrency();
        public double AccountEquity() => Client.AccountEquity();
        public int AccountLevarage() => Client.AccountLevarage();
        public string AccountName() => Client.AccountName();
        public int AccountNumber() => Client.AccountNumber();
        public string AccountServer() => Client.AccountServer();
        public DateTime ServerTime() => Client.ServerTime();
        public CtOrderData[] AccountHistory(DateTime from, DateTime to) => Client.AccountHistory(from, to);
        public CtOrderData[] AccountPositions() => Client.AccountPositions();
        public bool CloseAllPositions() => Client.CloseAllPositions();
        public bool ClosePosition(int ticket) => Client.ClosePosition(ticket);

        protected virtual void ConnectionChangedCallback(bool closed)
        {
            string message = "";
            if (closed)
            {
                _state = ConnectionState.Disconnected;
                message = "Server got disconnected";
            }
            else
            {
                _state = ConnectionState.Failed;
                message = "Server died";
            }
            Client.Dispose();
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(_state, message));
        }
        protected virtual void QuoteUpdateCallback(object sender, CtQuote e)
        {
            CtQuoteEventArgs args = new CtQuoteEventArgs
            {
                Symbol = e.Symbol,
                Ask = e.Ask,
                Bid = e.Bid,
            };
            OnQuote?.Invoke(sender, args);
        }
        protected virtual void PositionOpenCallback(object sender, CtOrderData e)
        {
            CtOrderDataEventArgs args = new CtOrderDataEventArgs
            {
                Ticket = e.Ticket,
                Symbol = e.Symbol,
                Type = e.Type,
                Volume = e.Volume,
                Profit = e.Profit,
                OpenPrice = e.OpenPrice,
                OpenTime = e.OpenTime,
                ClosePrice = e.ClosePrice,
                CloseTime = e.CloseTime,
                Comment = e.Comment,
                Commision = e.Commision,
                Swap = e.Swap,
                TakeProfit = e.TakeProfit,
                StopLoss = e.StopLoss,
            };
            OnPositionOpen?.Invoke(sender, args);
        }
        protected virtual void PositionCloseCallback(object sender, CtOrderData e)
        {
            CtOrderDataEventArgs args = new CtOrderDataEventArgs
            {
                Ticket = e.Ticket,
                Symbol = e.Symbol,
                Type = e.Type,
                Volume = e.Volume,
                Profit = e.Profit,
                OpenPrice = e.OpenPrice,
                OpenTime = e.OpenTime,
                ClosePrice = e.ClosePrice,
                CloseTime = e.CloseTime,
                Comment = e.Comment,
                Commision = e.Commision,
                Swap = e.Swap,
                TakeProfit = e.TakeProfit,
                StopLoss = e.StopLoss,
            };
            OnPositionClose?.Invoke(sender, args);
        }
        protected virtual void PositionModifyCallback(object sender, CtOrderData e)
        {
            CtOrderDataEventArgs args = new CtOrderDataEventArgs
            {
                Ticket = e.Ticket,
                Symbol = e.Symbol,
                Type = e.Type,
                Volume = e.Volume,
                Profit = e.Profit,
                OpenPrice = e.OpenPrice,
                OpenTime = e.OpenTime,
                ClosePrice = e.ClosePrice,
                CloseTime = e.CloseTime,
                Comment = e.Comment,
                Commision = e.Commision,
                Swap = e.Swap,
                TakeProfit = e.TakeProfit,
                StopLoss = e.StopLoss,
            };
            OnPositionModify?.Invoke(sender, args);
        }
    }

    public struct ConnectionStateEventArgs
    {
        public ConnectionState State { get; }
        public string Message { get; }

        public ConnectionStateEventArgs(ConnectionState state, string message)
        {
            State = state;
            Message = message;
        }
    }

    public enum ConnectionState
    {
        Connected,
        Connecting,
        Disconnected,
        Failed
    }
}
