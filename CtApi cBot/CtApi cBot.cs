using cAlgo.API;
using CTApiService;
using System;
using System.Linq;
using System.Collections.Generic;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class CtApicBot : Robot
    {
        [Parameter(DefaultValue = 2529)]
        public int Port { get; set; }

        public CtApiService service = null;
        public Dictionary<int, double> PosVolRec = null;

        protected override void OnStart()
        {
            PosVolRec = new Dictionary<int, double>();
            service = new CtApiService(this);
            Port = CtAdapter.GetInstance().AddBot(Port, service);
            PosVolRec.Clear();
            foreach(var pos in Positions) PosVolRec.Add(pos.Id, pos.Quantity);
            Positions.Opened += PositionsOpened;
            Positions.Modified += PositionsModified;
            Positions.Closed += PositionsClosed;
            Print(string.Format("Service running on localhost:{0}", Port));
        }

        protected override void OnTick()
        {
            if (service != null)
            {
                service.QuoteUpdateCallback(new CtQuoteData()
                {
                    Symbol = Symbol.Name.Replace("/", ""),
                    Bid = Symbol.Bid,
                    Ask = Symbol.Ask
                });
            }
        }

        protected override void OnStop()
        {
            Positions.Opened -= PositionsOpened;
            Positions.Modified -= PositionsModified;
            Positions.Closed -= PositionsClosed;
            CtAdapter.GetInstance().RemoveBot(Port);
            Print("Service stopped");
        }

        private void PositionsOpened(PositionOpenedEventArgs obj)
        {
            if (!PosVolRec.ContainsKey(obj.Position.Id)) PosVolRec.Add(obj.Position.Id, obj.Position.Quantity);
            if (service != null)
            {
                CtOrderData args = new CtOrderData 
                {
                    Ticket = obj.Position.Id,
                    ClosingTicket = 0,
                    Symbol = obj.Position.SymbolName.Replace("/", ""),
                    Type = obj.Position.TradeType.ToString(),
                    Volume = obj.Position.Quantity,
                    OpenPrice = obj.Position.EntryPrice,
                    OpenTime = obj.Position.EntryTime,
                    ClosePrice = 0.0,
                    CloseTime = DateTime.MinValue,
                    Comment = obj.Position.Comment,
                    Commission = obj.Position.Commissions,
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
                if (obj.Position.Quantity < PosVolRec[obj.Position.Id])
                {
                    var trade = History.LastOrDefault(a => a.PositionId == obj.Position.Id);
                    CtOrderData args = new CtOrderData
                    {
                        Ticket = trade.PositionId,
                        ClosingTicket = trade.ClosingDealId,
                        Symbol = trade.SymbolName.Replace("/", ""),
                        Type = trade.TradeType.ToString(),
                        Volume = trade.Quantity,
                        OpenPrice = trade.EntryPrice,
                        OpenTime = trade.EntryTime,
                        ClosePrice = trade.ClosingPrice,
                        CloseTime = trade.ClosingTime,
                        Comment = trade.Comment,
                        Commission = trade.Commissions,
                        Swap = trade.Swap,
                        Profit = trade.NetProfit
                    };
                    args.StopLoss = obj.Position.StopLoss.HasValue ? obj.Position.StopLoss.Value : 0.0;
                    args.TakeProfit = obj.Position.TakeProfit.HasValue ? obj.Position.TakeProfit.Value : 0.0;
                    service.PositionCloseCallback(args);
                }
                else
                {
                    CtOrderData args = new CtOrderData
                    {
                        Ticket = obj.Position.Id,
                        ClosingTicket = 0,
                        Symbol = obj.Position.SymbolName.Replace("/", ""),
                        Type = obj.Position.TradeType.ToString(),
                        Volume = obj.Position.Quantity,
                        OpenPrice = obj.Position.EntryPrice,
                        OpenTime = obj.Position.EntryTime,
                        ClosePrice = 0.0,
                        CloseTime = DateTime.MinValue,
                        Comment = obj.Position.Comment,
                        Commission = obj.Position.Commissions,
                        Swap = obj.Position.Swap,
                        Profit = obj.Position.NetProfit
                    };
                    args.StopLoss = obj.Position.StopLoss.HasValue ? obj.Position.StopLoss.Value : 0.0;
                    args.TakeProfit = obj.Position.TakeProfit.HasValue ? obj.Position.TakeProfit.Value : 0.0;
                    service.PositionModifyCallback(args);
                }
            }
            PosVolRec[obj.Position.Id] = obj.Position.Quantity;
        }
        private void PositionsClosed(PositionClosedEventArgs obj)
        {
            if (service != null)
            {
                var trade = History.LastOrDefault(a => a.PositionId == obj.Position.Id);
                CtOrderData args = new CtOrderData 
                {
                    Ticket = trade.PositionId,
                    ClosingTicket = trade.ClosingDealId,
                    Symbol = trade.SymbolName.Replace("/", ""),
                    Type = trade.TradeType.ToString(),
                    Volume = trade.Quantity,
                    OpenPrice = trade.EntryPrice,
                    OpenTime = trade.EntryTime,
                    ClosePrice = trade.ClosingPrice,
                    CloseTime = trade.ClosingTime,
                    Comment = trade.Comment,
                    Commission = trade.Commissions,
                    Swap = trade.Swap,
                    Profit = trade.NetProfit
                };
                args.StopLoss = obj.Position.StopLoss.HasValue ? obj.Position.StopLoss.Value : 0.0;
                args.TakeProfit = obj.Position.TakeProfit.HasValue ? obj.Position.TakeProfit.Value : 0.0;
                service.PositionCloseCallback(args);
            }
            if (PosVolRec.ContainsKey(obj.Position.Id)) PosVolRec.Remove(obj.Position.Id);
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
                return cBot != null ? cBot.Account.UserId.ToString() : "";
            }

            public override int AccountNumber()
            {
                return cBot != null ? (int)cBot.Account.Number : 0;
            }
            public override string AccountServer()
            {
                return cBot != null ? cBot.Account.IsLive ? "Live" : "Demo" : "";
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
                            ClosingTicket = 0,
                            Symbol = position.SymbolName.Replace("/", ""),
                            Type = position.TradeType.ToString(),
                            Volume = position.Quantity,
                            Profit = position.NetProfit,
                            OpenPrice = position.EntryPrice,
                            OpenTime = position.EntryTime,
                            ClosePrice = 0.0,
                            CloseTime = DateTime.MinValue,
                            Comment = position.Comment,
                            Commission = position.Commissions,
                            Swap = position.Swap
                        };
                        data.StopLoss = position.StopLoss.HasValue ? position.StopLoss.Value : 0;
                        data.TakeProfit = position.TakeProfit.HasValue ? position.TakeProfit.Value : 0;
                        positions.Add(data);
                    }
                }
                return positions;
            }
            public override List<CtOrderData> AccountHistory(DateTime start, DateTime end)
            {
                List<CtOrderData> history = new List<CtOrderData>();
                if (cBot != null)
                {
                    foreach (var order in cBot.History)
                    {
                        var orderDateTime = order.ClosingTime;
                        if (start.Millisecond == 0 && end.Millisecond == 0)
                            orderDateTime = orderDateTime.AddMilliseconds(-1 * orderDateTime.Millisecond);
                        if (orderDateTime >= start && orderDateTime <= end)
                        {
                            var data = new CtOrderData 
                            {
                                Ticket = order.PositionId,
                                ClosingTicket = order.ClosingDealId,
                                Symbol = order.SymbolName.Replace("/", ""),
                                Type = order.TradeType.ToString(),
                                Volume = order.Quantity,
                                Profit = order.NetProfit,
                                OpenPrice = order.EntryPrice,
                                OpenTime = order.EntryTime,
                                ClosePrice = order.ClosingPrice,
                                CloseTime = order.ClosingTime,
                                Comment = order.Comment,
                                Commission = order.Commissions,
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
