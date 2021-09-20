using cAlgo.API;
using CTApiService;
using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class CtApicBot : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public int Port { get; set; }

        public CtApiService service = null;

        protected override void OnStart()
        {
            service = new CtApiService(this);
            Port = CtAdapter.GetInstance().AddBot(Port, service);
            Positions.Opened += PositionsOpened;
            Positions.Modified += PositionsModified;
            Positions.Closed += PositionsClosed;
            MessageBox.Show(string.Format("Service running on localhost:{0}", Port));
        }

        protected override void OnTick()
        {
            if (service != null)
            {
                service.QuoteUpdateCallback(new CtQuote());
            }
        }

        protected override void OnStop()
        {
            Positions.Opened -= PositionsOpened;
            Positions.Modified -= PositionsModified;
            Positions.Closed -= PositionsClosed;
            CtAdapter.GetInstance().RemoveBot(Port);
            MessageBox.Show("Service stopped");
        }

        private void PositionsOpened(PositionOpenedEventArgs obj)
        {
            if (service != null)
            {
                CtOrderData args = new CtOrderData 
                {
                    Ticket = obj.Position.Id,
                    Symbol = obj.Position.SymbolName,
                    Type = obj.Position.TradeType.ToString(),
                    Volume = obj.Position.Quantity,
                    OpenPrice = obj.Position.EntryPrice,
                    OpenTime = obj.Position.EntryTime,
                    ClosePrice = 0.0,
                    CloseTime = DateTime.Now,
                    Comment = obj.Position.Comment,
                    Commision = obj.Position.Commissions,
                    Swap = obj.Position.Swap,
                    Profit = obj.Position.NetProfit
                };
                args.StopLoss = obj.Position.StopLoss.HasValue ? obj.Position.StopLoss.Value : 0.0;
                args.TakeProfit = obj.Position.TakeProfit.HasValue ? obj.Position.TakeProfit.Value : 0.0;
                service.PositionOpenCallback(args);
            }
        }
        private void PositionsModified(PositionModifiedEventArgs obj)
        {
            if (service != null)
            {
                CtOrderData args = new CtOrderData 
                {
                    Ticket = obj.Position.Id,
                    Symbol = obj.Position.SymbolName,
                    Type = obj.Position.TradeType.ToString(),
                    Volume = obj.Position.VolumeInUnits,
                    OpenPrice = obj.Position.EntryPrice,
                    OpenTime = obj.Position.EntryTime,
                    ClosePrice = 0.0,
                    CloseTime = DateTime.Now,
                    Comment = obj.Position.Comment,
                    Commision = obj.Position.Commissions,
                    Swap = obj.Position.Swap,
                    Profit = obj.Position.NetProfit
                };
                args.StopLoss = obj.Position.StopLoss.HasValue ? obj.Position.StopLoss.Value : 0.0;
                args.TakeProfit = obj.Position.TakeProfit.HasValue ? obj.Position.TakeProfit.Value : 0.0;
                service.PositionModifyCallback(args);
            }
        }
        private void PositionsClosed(PositionClosedEventArgs obj)
        {
            var trade = History.First(a => a.PositionId == obj.Position.Id);
            if (service != null)
            {
                CtOrderData args = new CtOrderData 
                {
                    Ticket = obj.Position.Id,
                    Symbol = trade.SymbolName,
                    Type = trade.TradeType.ToString(),
                    Volume = trade.VolumeInUnits,
                    OpenPrice = trade.EntryPrice,
                    OpenTime = trade.EntryTime,
                    ClosePrice = trade.ClosingPrice,
                    CloseTime = trade.ClosingTime,
                    Comment = trade.Comment,
                    Commision = trade.Commissions,
                    Swap = trade.Swap,
                    Profit = trade.NetProfit
                };
                args.StopLoss = obj.Position.StopLoss.HasValue ? obj.Position.StopLoss.Value : 0.0;
                args.TakeProfit = obj.Position.TakeProfit.HasValue ? obj.Position.TakeProfit.Value : 0.0;
                service.PositionCloseCallback(args);
            }
        }

        public class CtApiService : CtServiceBase
        {
            private Robot cBot = null;
            public CtApiService(Robot bot) : base()
            {
                cBot = bot;
            }

            public override double AccountBalance()
            {
                return cBot != null ? cBot.Account.Balance : 0;
            }

            public override string AccountCompany()
            {
                return cBot != null ? cBot.Account.BrokerName : "";
            }

            public override string AccountCurrency()
            {
                return cBot != null ? cBot.Account.Asset.Name : "";
            }

            public override double AccountEquity()
            {
                return cBot != null ? cBot.Account.Equity : 0;
            }

            public override int AccountLevarage()
            {
                return cBot != null ? (int)cBot.Account.PreciseLeverage : 0;
            }

            public override string AccountName()
            {
                return "";
            }

            public override int AccountNumber()
            {
                return cBot != null ? (int)cBot.Account.UserId : 0;
            }
            public override string AccountServer()
            {
                return cBot != null ? cBot.Server.ToString() : "";
            }
            public override DateTime ServerTime()
            {
                return cBot != null ? cBot.Server.Time : DateTime.MinValue;
            }

            public override List<CtOrderData> AccountPositions()
            {
                List<CtOrderData> positions = new List<CtOrderData>();
                if (cBot != null)
                {
                    foreach (var position in cBot.Positions)
                    {
                        var data = new CtOrderData 
                        {
                            Ticket = position.Id,
                            Symbol = position.SymbolName,
                            Type = position.TradeType.ToString(),
                            Volume = position.VolumeInUnits,
                            Profit = position.NetProfit,
                            OpenPrice = position.EntryPrice,
                            OpenTime = position.EntryTime,
                            ClosePrice = 0.0,
                            CloseTime = DateTime.Now,
                            Comment = position.Comment,
                            Commision = position.Commissions,
                            Swap = position.Swap
                        };
                        data.StopLoss = position.StopLoss.HasValue ? position.StopLoss.Value : 0;
                        data.TakeProfit = position.TakeProfit.HasValue ? position.TakeProfit.Value : 0;
                        positions.Add(data);
                    }
                }
                return positions;
            }
            public override List<CtOrderData> AccountHistory(DateTime @from, DateTime to)
            {
                List<CtOrderData> history = new List<CtOrderData>();
                if (cBot != null)
                {
                    foreach (var order in cBot.History)
                    {
                        if (order.EntryTime >= @from && order.EntryTime <= to)
                        {
                            var data = new CtOrderData 
                            {
                                Ticket = order.PositionId,
                                Symbol = order.SymbolName,
                                Type = order.TradeType.ToString(),
                                Volume = order.VolumeInUnits,
                                Profit = order.NetProfit,
                                OpenPrice = order.EntryPrice,
                                OpenTime = order.EntryTime,
                                ClosePrice = 0.0,
                                CloseTime = DateTime.Now,
                                Comment = order.Comment,
                                Commision = order.Commissions,
                                Swap = order.Swap,
                                StopLoss = 0,
                                TakeProfit = 0
                            };
                            history.Add(data);
                        }
                    }
                }
                return history;
            }
            public override bool CloseAllPositions()
            {
                bool retval = false;
                if (cBot != null)
                {
                    retval = true;
                    foreach (var position in cBot.Positions.ToList())
                    {
                        var res = position.Close();
                        if (!res.IsSuccessful)
                            retval = false;
                    }
                }
                return retval;
            }
            public override bool ClosePosition(int ticket)
            {
                bool retval = false;
                if (cBot != null)
                {
                    foreach (var position in cBot.Positions.ToList())
                    {
                        if (position.Id == ticket)
                        {
                            var res = position.Close();
                            retval = res.IsSuccessful;
                        }
                    }
                }
                return retval;
            }
        }
    }
}
