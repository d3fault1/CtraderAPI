using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace CTApiService
{

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, AutomaticSessionShutdown = true, IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public abstract class CtServiceBase : ICtApi
    {
        private ReaderWriterLock _clientslocker;
        private List<ICtApiCallback> _clients;

        public CtServiceBase()
        {
            _clientslocker = new ReaderWriterLock();
            _clients = new List<ICtApiCallback>();
        }

        public bool Connect()
        {
            var client = OperationContext.Current.GetCallbackChannel<ICtApiCallback>();
            OperationContext.Current.Channel.Closed += ChannelClosed;
            OperationContext.Current.Channel.Faulted += ChannelFaulted;
            return clientListManager(client, false);
        }

        public void Disconnect()
        {
            var client = OperationContext.Current.GetCallbackChannel<ICtApiCallback>();
            clientListManager(client, true);
        }

        private void ChannelClosed(object sender, EventArgs e)
        {
            clientListManager((ICtApiCallback)sender, true);
        }

        private void ChannelFaulted(object sender, EventArgs e)
        {
            clientListManager((ICtApiCallback)sender, true);
        }

        private bool clientListManager(ICtApiCallback client, bool remove)
        {
            if (client == null) return false;

            var retval = false;

            try
            {
                _clientslocker.AcquireWriterLock(10000);

                try
                {
                    if (remove)
                    {
                        if (_clients.Contains(client)) _clients.Remove(client);
                    }
                    else
                    {
                        if (!_clients.Contains(client)) _clients.Add(client);
                    }
                    retval = true;
                }
                finally
                {
                    _clientslocker.ReleaseWriterLock();
                }
            }
            catch (ApplicationException ex)
            {

            }

            return retval;
        }

        public abstract double AccountBalance();
        public abstract string AccountCompany();
        public abstract string AccountCurrency();
        public abstract double AccountEquity();
        public abstract int AccountLevarage();
        public abstract string AccountName();
        public abstract int AccountNumber();
        public abstract string AccountServer();
        public abstract DateTime ServerTime();
        public abstract List<CtOrderData> AccountHistory(DateTime from, DateTime to);
        public abstract List<CtOrderData> AccountPositions();
        public abstract bool CloseAllPositions();
        public abstract bool ClosePosition(int ticket);

        public void QuoteUpdateCallback(CtQuoteData quote)
        {
            try
            {
                _clientslocker.AcquireReaderLock(2000);
                try
                {
                    foreach (var client in _clients)
                    {
                        try
                        {
                            client.QuoteOccured(quote);
                        }
                        catch (Exception e)
                        {
                            return;
                        }
                    }
                }
                finally
                {
                    _clientslocker.ReleaseReaderLock();
                }
            }
            catch (ApplicationException ex)
            {
                return;
            }
        }
        public void PositionOpenCallback(CtOrderData position)
        {
            try
            {
                _clientslocker.AcquireReaderLock(2000);
                try
                {
                    foreach (var client in _clients)
                    {
                        try
                        {
                            client.PositionOpenOccured(position);
                        }
                        catch (Exception e)
                        {
                            return;
                        }
                    }
                }
                finally
                {
                    _clientslocker.ReleaseReaderLock();
                }
            }
            catch (ApplicationException ex)
            {
                return;
            }
        }
        public void PositionCloseCallback(CtOrderData position)
        {
            try
            {
                _clientslocker.AcquireReaderLock(2000);
                try
                {
                    foreach (var client in _clients)
                    {
                        try
                        {
                            client.PositionCloseOccured(position);
                        }
                        catch (Exception e)
                        {
                            return;
                        }
                    }
                }
                finally
                {
                    _clientslocker.ReleaseReaderLock();
                }
            }
            catch (ApplicationException ex)
            {
                return;
            }
        }
        public void PositionModifyCallback(CtOrderData position)
        {
            try
            {
                _clientslocker.AcquireReaderLock(2000);
                try
                {
                    foreach (var client in _clients)
                    {
                        try
                        {
                            client.PositionModifyOccured(position);
                        }
                        catch (Exception e)
                        {
                            return;
                        }
                    }
                }
                finally
                {
                    _clientslocker.ReleaseReaderLock();
                }
            }
            catch (ApplicationException ex)
            {
                return;
            }
        }
    }
}
