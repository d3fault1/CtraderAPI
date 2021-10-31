using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CTApiService;

namespace CTApi
{
    public class CtApi
    {
        private readonly object _locker = new object();
        private CtClient Client;
        private CtConnectionState _state = CtConnectionState.Disconnected;

        public CtConnectionState State
        {
            get
            {
                return _state;
            }
        }

        #region Events and Delegates
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
        #endregion

        public CtApi()
        {

        }

        public void BeginConnect(int port)
        {
            Task.Factory.StartNew(() => Connect(port));
        }

        public void BeginDisconnect()
        {
            Task.Factory.StartNew(() => Disconnect());
        }

        private void Connect(int port)
        {
            lock (_locker)
            {
                if (_state == CtConnectionState.Connected || _state == CtConnectionState.Connecting)
                {
                    return;
                }

                _state = CtConnectionState.Connecting;
            }

            Client = new CtClient(port);

            string message = $"Connecting to localhost:{port}";
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(_state, message));

            lock (_locker)
            {
                var state = CtConnectionState.Failed;

                try
                {
                    Client.Connect();
                    state = CtConnectionState.Connected;
                }
                catch (Exception e)
                {
                    Client.Dispose();
                    message = $"Failed connection to localhost:{port}. Server not running.";
                }

                if (state == CtConnectionState.Connected)
                {
                    Client.Interrupted += DisconnectedCallback;
                    Client.OnQuote += QuoteUpdateCallback;
                    Client.OnPositionOpen += PositionOpenCallback;
                    Client.OnPositionModify += PositionModifyCallback;
                    Client.OnPositionClose += PositionCloseCallback;

                    message = $"Connected to localhost:{port}";
                }
                _state = state;
            }

            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(_state, message));
        }

        private void Disconnect()
        {
            string message = "";

            lock (_locker)
            {
                var state = CtConnectionState.Disconnected;
                message = $"Server disconnected successfully";
                if (!Client.Disconnect())
                {
                    state = CtConnectionState.Failed;
                    message = $"Server disconnection failed";
                }
                Client.Interrupted -= DisconnectedCallback;
                Client.OnQuote -= QuoteUpdateCallback;
                Client.OnPositionOpen -= PositionOpenCallback;
                Client.OnPositionModify -= PositionModifyCallback;
                Client.OnPositionClose -= PositionCloseCallback;

                Client.Dispose();
                _state = state;
            }

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
        public CtOrder[] AccountHistory(DateTime from, DateTime to)
        {
            List<CtOrder> retlist = new List<CtOrder>();
            foreach (var order in Client.AccountHistory(from, to))
            {
                var ctorder = new CtOrder
                {
                    Ticket = order.Ticket,
                    ClosingTicket = order.ClosingTicket,
                    Symbol = order.Symbol,
                    Type = order.Type,
                    Profit = order.Profit,
                    Volume = order.Volume,
                    OpenPrice = order.OpenPrice,
                    OpenTime = order.OpenTime,
                    ClosePrice = order.ClosePrice,
                    CloseTime = order.CloseTime,
                    Comment = order.Comment,
                    Commission = order.Commission,
                    Swap = order.Swap,
                    StopLoss = order.StopLoss,
                    TakeProfit = order.TakeProfit
                };
                retlist.Add(ctorder);
            }
            return retlist.ToArray();
        }
        public CtOrder[] AccountPositions()
        {
            List<CtOrder> retlist = new List<CtOrder>();
            foreach (var order in Client.AccountPositions())
            {
                var ctorder = new CtOrder
                {
                    Ticket = order.Ticket,
                    ClosingTicket = order.ClosingTicket,
                    Symbol = order.Symbol,
                    Type = order.Type,
                    Profit = order.Profit,
                    Volume = order.Volume,
                    OpenPrice = order.OpenPrice,
                    OpenTime = order.OpenTime,
                    ClosePrice = order.ClosePrice,
                    CloseTime = order.CloseTime,
                    Comment = order.Comment,
                    Commission = order.Commission,
                    Swap = order.Swap,
                    StopLoss = order.StopLoss,
                    TakeProfit = order.TakeProfit
                };
                retlist.Add(ctorder);
            }
            return retlist.ToArray();
        }
        public bool CloseAllPositions() => Client.CloseAllPositions();
        public bool ClosePosition(int ticket) => Client.ClosePosition(ticket);

        protected virtual void DisconnectedCallback(bool closed)
        {
            string message = "";

            lock (_locker)
            {
                var state = CtConnectionState.Disconnected;
                if (closed)
                {
                    message = "Server disconnected";
                }
                else
                {
                    state = CtConnectionState.Failed;
                    message = "Server died";
                }

                Client.Interrupted -= DisconnectedCallback;
                Client.OnQuote -= QuoteUpdateCallback;
                Client.OnPositionOpen -= PositionOpenCallback;
                Client.OnPositionModify -= PositionModifyCallback;
                Client.OnPositionClose -= PositionCloseCallback;

                Client.Dispose();
                _state = state;
            }

            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(_state, message));
        }
        protected virtual void QuoteUpdateCallback(object sender, CtQuoteData e)
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
                ClosingTicket = e.ClosingTicket,
                Symbol = e.Symbol,
                Type = e.Type,
                Volume = e.Volume,
                Profit = e.Profit,
                OpenPrice = e.OpenPrice,
                OpenTime = e.OpenTime,
                ClosePrice = e.ClosePrice,
                CloseTime = e.CloseTime,
                Comment = e.Comment,
                Commission = e.Commission,
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
                ClosingTicket = e.ClosingTicket,
                Symbol = e.Symbol,
                Type = e.Type,
                Volume = e.Volume,
                Profit = e.Profit,
                OpenPrice = e.OpenPrice,
                OpenTime = e.OpenTime,
                ClosePrice = e.ClosePrice,
                CloseTime = e.CloseTime,
                Comment = e.Comment,
                Commission = e.Commission,
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
                ClosingTicket = e.ClosingTicket,
                Symbol = e.Symbol,
                Type = e.Type,
                Volume = e.Volume,
                Profit = e.Profit,
                OpenPrice = e.OpenPrice,
                OpenTime = e.OpenTime,
                ClosePrice = e.ClosePrice,
                CloseTime = e.CloseTime,
                Comment = e.Comment,
                Commission = e.Commission,
                Swap = e.Swap,
                TakeProfit = e.TakeProfit,
                StopLoss = e.StopLoss,
            };
            OnPositionModify?.Invoke(sender, args);
        }
    }
}
