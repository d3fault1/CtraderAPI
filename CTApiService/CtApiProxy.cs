using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CTApiService
{
    internal class CtApiProxy : DuplexClientBase<ICtApi>, IDisposable
    {
        public event EventHandler Faulted;
        public event EventHandler Closed;
        public CtApiProxy(InstanceContext context, Binding binding, EndpointAddress address) : base(context, binding, address)
        {
            InnerDuplexChannel.Faulted += InnerDuplexChannelFaulted;
            InnerDuplexChannel.Closed += InnerDuplexChannelClosed;
        }

        public bool Connect()
        {
            InnerDuplexChannel.Open();
            return Channel.Connect();
        }

        public void Disconnect()
        {
            Channel.Disconnect();
        }

        public double AccountBalance() => Channel.AccountBalance();
        public string AccountCompany() => Channel.AccountCompany();
        public string AccountCurrency() => Channel.AccountCurrency();
        public double AccountEquity() => Channel.AccountEquity();
        public int AccountLevarage() => Channel.AccountLevarage();
        public string AccountName() => Channel.AccountName();
        public int AccountNumber() => Channel.AccountNumber();
        public string AccountServer() => Channel.AccountServer();
        public DateTime ServerTime() => Channel.ServerTime();
        public CtOrderData[] AccountHistory(DateTime from, DateTime to) => Channel.AccountHistory(from, to).ToArray();
        public CtOrderData[] AccountPositions() => Channel.AccountPositions().ToArray();
        public bool CloseAllPositions() => Channel.CloseAllPositions();
        public bool ClosePosition(int ticket) => Channel.ClosePosition(ticket);

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch (CommunicationException)
            {
                Abort();
            }
            catch (TimeoutException)
            {
                Abort();
            }
            catch (Exception)
            {
                Abort();
            }
        }

        private void InnerDuplexChannelClosed(object sender, EventArgs e)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void InnerDuplexChannelFaulted(object sender, EventArgs e)
        {
            Faulted?.Invoke(this, EventArgs.Empty);
        }
    }
}
