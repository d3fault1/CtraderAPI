using System;
using System.ServiceModel;

namespace CTApiService
{
    public class CtClient : ICtApiCallback, IDisposable
    {
        private readonly CtApiProxy _proxy;

        private bool IsConnected => _proxy.State == CommunicationState.Opened;

        public delegate void CommunicationEventHandler(bool closed);
        public delegate void QuoteEventHandler(object sender, CtQuote e);
        public delegate void PositionOpenEventHandler(object sender, CtOrderData e);
        public delegate void PositionCloseEventHandler(object sender, CtOrderData e);
        public delegate void PositionModifiedEventHandler(object sender, CtOrderData e);

        public event CommunicationEventHandler Interrupted;
        public event QuoteEventHandler OnQuote;
        public event PositionOpenEventHandler OnPositionOpen;
        public event PositionCloseEventHandler OnPositionClose;
        public event PositionModifiedEventHandler OnPositionModify;

        public CtClient(int port)
        {
            if (port < 1000 || port > 9999)
                throw new ArgumentOutOfRangeException(nameof(port), "port value is invalid");

            var urlService = $"net.pipe://localhost/MtApiService_{port}";

            var bind = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                MaxReceivedMessageSize = 2147483647,
                MaxBufferSize = 2147483647,
                MaxBufferPoolSize = 2147483647,
                SendTimeout = new TimeSpan(12, 0, 0),
                ReceiveTimeout = new TimeSpan(12, 0, 0),
                ReaderQuotas =
                {
                    MaxArrayLength = 2147483647,
                    MaxBytesPerRead = 2147483647,
                    MaxDepth = 2147483647,
                    MaxStringContentLength = 2147483647,
                    MaxNameTableCharCount = 2147483647
                }
            };

            _proxy = new CtApiProxy(new InstanceContext(this), bind, new EndpointAddress(urlService));
            _proxy.Faulted += _proxyFaulted;
            _proxy.Closed += _proxyClosed;
        }

        public bool Connect()
        {
            //Log.Debug("Connect: begin.");

            if (_proxy.State != CommunicationState.Created)
            {
                //Log.ErrorFormat("Connected: end. Client has invalid state {0}", _proxy.State);
                return false;
            }

            bool coonected;

            try
            {
                coonected = _proxy.Connect();
            }
            catch (Exception ex)
            {
                //Log.ErrorFormat("Connect: Exception - {0}", ex.Message);

                throw new CommunicationException($"Connection failed to service. {ex.Message}");
            }

            if (coonected == false)
            {
                //Log.Error("Connect: end. Connection failed.");
                throw new CommunicationException("Connection failed");
            }
            return true;

            //Log.Debug("Connect: end.");
        }

        public bool Disconnect()
        {
            //Log.Debug("Disconnect: begin.");

            try
            {
                _proxy.Disconnect();
            }
            catch (Exception ex)
            {
                return false;
                //Log.ErrorFormat("Disconnect: Exception - {0}", ex.Message);
            }
            return true;
            //Log.Debug("Disconnect: end.");
        }

        public double AccountBalance()
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.AccountBalance();
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public string AccountCompany()
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.AccountCompany();
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public string AccountCurrency()
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.AccountCurrency();
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public double AccountEquity()
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.AccountEquity();
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public int AccountLevarage()
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.AccountLevarage();
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public string AccountName()
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.AccountName();
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public int AccountNumber()
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.AccountNumber();
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public string AccountServer()
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.AccountServer();
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public DateTime ServerTime()
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.ServerTime();
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public CtOrderData[] AccountPositions()
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.AccountPositions();
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public CtOrderData[] AccountHistory(DateTime from, DateTime to)
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.AccountHistory(from, to);
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public bool CloseAllPositions()
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.CloseAllPositions();
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }
        public bool ClosePosition(int ticket)
        {
            if (!IsConnected) throw new CommunicationException("Client is not connected");
            try
            {
                return _proxy.ClosePosition(ticket);
            }
            catch (Exception ex)
            {
                throw new CommunicationException("Service connection failed! " + ex.Message);
            }
        }

        public void PositionCloseOccured(CtOrderData position)
        {
            CtOrderData args = new CtOrderData
            {
                Ticket = position.Ticket,
                Symbol = position.Symbol,
                Type = position.Type,
                Volume = position.Volume,
                Profit = position.Profit,
                OpenPrice = position.OpenPrice,
                OpenTime = position.OpenTime,
                ClosePrice = position.ClosePrice,
                CloseTime = position.CloseTime,
                TakeProfit = position.TakeProfit,
                StopLoss = position.StopLoss,
                Comment = position.Comment,
                Commision = position.Commision,
                Swap = position.Swap
            };
            OnPositionClose?.Invoke(this, args);
        }
        public void PositionModifiedOccured(CtOrderData position)
        {
            CtOrderData args = new CtOrderData
            {
                Ticket = position.Ticket,
                Symbol = position.Symbol,
                Type = position.Type,
                Volume = position.Volume,
                Profit = position.Profit,
                OpenPrice = position.OpenPrice,
                OpenTime = position.OpenTime,
                ClosePrice = position.ClosePrice,
                CloseTime = position.CloseTime,
                TakeProfit = position.TakeProfit,
                StopLoss = position.StopLoss,
                Comment = position.Comment,
                Commision = position.Commision,
                Swap = position.Swap
            };
            OnPositionModify?.Invoke(this, args);
        }
        public void PositionOpenOccured(CtOrderData position)
        {
            CtOrderData args = new CtOrderData
            {
                Ticket = position.Ticket,
                Symbol = position.Symbol,
                Type = position.Type,
                Volume = position.Volume,
                Profit = position.Profit,
                OpenPrice = position.OpenPrice,
                OpenTime = position.OpenTime,
                ClosePrice = position.ClosePrice,
                CloseTime = position.CloseTime,
                TakeProfit = position.TakeProfit,
                StopLoss = position.StopLoss,
                Comment = position.Comment,
                Commision = position.Commision,
                Swap = position.Swap
            };
            OnPositionOpen?.Invoke(this, args);
        }
        public void QuoteOccured(CtQuote quote)
        {
            CtQuote args = new CtQuote
            {
                Symbol = quote.Symbol,
                Ask = quote.Ask,
                Bid = quote.Bid
            };
            OnQuote?.Invoke(this, args);
        }

        public void Dispose()
        {
            _proxy.Dispose();
        }

        private void _proxyClosed(object sender, EventArgs e)
        {
            Interrupted?.Invoke(true);
        }

        private void _proxyFaulted(object sender, EventArgs e)
        {
            Interrupted?.Invoke(false);
        }
    }
}
