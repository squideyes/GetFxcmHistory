// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using fxcore2;
using System;
using System.Collections.Generic;

namespace GetFxcmHistory.FXCM
{
    public class Fetcher
    {
        public class ProgressArgs : EventArgs
        {
            internal ProgressArgs(int blockId, List<Tick> ticks)
            {
                BlockId = blockId;
                TickCount = ticks.Count;
                MaxTickOn = ticks[0].TickOn;
                MinTickOn = ticks[ticks.Count - 1].TickOn;
            }

            public int BlockId { get; }
            public int TickCount { get; }
            public DateTime MinTickOn { get; }
            public DateTime MaxTickOn { get; }
        }

        public class TickSetArgs : EventArgs
        {
            internal TickSetArgs(TickSet tickSet)
            {
                TickSet = tickSet;
            }

            public TickSet TickSet { get; }
        }

        private const int MAXBARS = 300;
        private const string URL = "http://www.fxcorporate.com/Hosts.jsp";

        private string userName;
        private string password;
        private string connection;

        public event EventHandler<StatusArgs> OnStatus;
        public event EventHandler<ProgressArgs> OnProgress;
        public event EventHandler<TickSetArgs> OnTickSet;
        public event EventHandler<ErrorArgs> OnError;

        public Fetcher(
            string userName, string password, Connection connection = Connection.Demo)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            if (!connection.IsDefinedEnum())
                throw new ArgumentOutOfRangeException(nameof(connection));

            this.userName = userName;
            this.password = password;
            this.connection = connection.ToString();
        }

        public void Fetch(Symbol symbol, DateTime date)
        {
            if (!symbol.IsDefinedEnum())
                throw new ArgumentOutOfRangeException(nameof(symbol));

            if (date.Kind != DateTimeKind.Unspecified)
                throw new ArgumentException("\"date.Kind\" must be \"Unspecified\"!");

            var ticks = new SortedDictionary<DateTime, Tick>();

            O2GSession session = null;

            try
            {
                session = O2GTransport.createSession();

                var statusListener = new StatusListener(session);

                statusListener.OnStatus += (s, e) =>
                    OnStatus?.Invoke(this, new StatusArgs((Status)(int)e.Status));

                statusListener.OnError += (s, e) =>
                    OnError?.Invoke(this, new ErrorArgs(e.Kind, e.Message));

                session.subscribeSessionStatus(statusListener);

                statusListener.Reset();

                session.login(userName, password, URL, connection);

                if (statusListener.WaitEvents() && statusListener.IsConnected)
                {
                    var canUnsubscribeResponse = false;

                    ResponseListener responseListener = null;

                    try
                    {
                        responseListener = new ResponseListener(session);

                        responseListener.OnRequestFailed += (s, e) =>
                            OnError?.Invoke(this, new ErrorArgs(ErrorKind.Session, e.ErrorMessage));

                        //????????????????????
                        responseListener.OnNoTicks += (s, e) =>
                            {
                            };

                        session.subscribeResponse(responseListener);

                        canUnsubscribeResponse = true;

                        GetTicks(ticks, symbol, date, session, responseListener);

                        var tickSet = new TickSet(symbol, date);

                        tickSet.Load(ticks.Values);

                        OnTickSet?.Invoke(this, new TickSetArgs(tickSet));
                    }
                    catch (Exception error)
                    {
                        OnError?.Invoke(this, new ErrorArgs(ErrorKind.Client, error.Message));
                    }
                    finally
                    {
                        statusListener.Reset();

                        session.logout();

                        statusListener.WaitEvents();

                        if (canUnsubscribeResponse)
                            session.unsubscribeResponse(responseListener);
                    }
                }

                session.unsubscribeSessionStatus(statusListener);
            }
            catch (TimeoutException error)
            {
                OnError?.Invoke(this, new ErrorArgs(ErrorKind.Timeout, error.Message));
            }
            catch (Exception error)
            {
                OnError?.Invoke(this, new ErrorArgs(ErrorKind.Client, error.Message));
            }
            finally
            {
                if (session != null)
                    session.Dispose();
            }
        }

        private void GetTicks(SortedDictionary<DateTime, Tick> ticks, Symbol symbol,
            DateTime date, O2GSession session, ResponseListener responseListener)
        {
            var requestFactory = session.getRequestFactory();

            var timeframe = requestFactory.Timeframes["t1"];

            if (timeframe == null)
                throw new Exception($"The \"t1\" timeframe is invalid!");

            var request = requestFactory.createMarketDataSnapshotRequestInstrument(
                symbol.ToInstrument(), timeframe, 300);

            var from = date.ToUtcFromEst();

            var until = date.AddDays(1).AddMilliseconds(-1).ToUtcFromEst();

            DateTime first = until;

            int blockId = 0;

            do
            {
                requestFactory.fillMarketDataSnapshotRequestTime(
                    request, from, first, false);

                responseListener.SetRequestID(request.RequestID);

                session.sendRequest(request);

                if (!responseListener.WaitEvents())
                    throw new TimeoutException("The ResponseListener timeout expired!");

                var response = responseListener.GetResponse();

                if ((response != null) && (response.Type == O2GResponseType.MarketDataSnapshot))
                {
                    var readerFactory = session.getResponseReaderFactory();

                    if (readerFactory != null)
                    {
                        var reader = readerFactory.createMarketDataSnapshotReader(response);

                        if (reader.Count > 0)
                        {
                            if (DateTime.Compare(first, reader.getDate(0)) != 0)
                                first = reader.getDate(0);
                            else
                                break;
                        }
                        else
                        {
                            break;
                        }
                    }

                    var block = ReadTicks(session, response, symbol, blockId++);

                    OnProgress?.Invoke(this, new ProgressArgs(blockId, block));

                    foreach (var tick in block)
                    {
                        if (ticks.ContainsKey(tick.TickOn))
                            ticks[tick.TickOn] = tick;
                        else
                            ticks.Add(tick.TickOn, tick);
                    }
                }
                else
                {
                    break;
                }
            }
            while (first.IgnoreMilliseconds() > from.IgnoreMilliseconds());
        }

        private List<Tick> ReadTicks(O2GSession session, O2GResponse response, Symbol symbol, int blockId)
        {
            var ticks = new List<Tick>();

            var factory = session.getResponseReaderFactory();

            if (factory != null)
            {
                var reader = factory.createMarketDataSnapshotReader(response);

                for (int i = reader.Count - 1; i >= 0; i--)
                {
                    if (reader.isBar)
                    {
                        throw new Exception(
                            "The prices were unexpectedly returned in \"bar\" format!");
                    }

                    var tick = new Tick()
                    {
                        Symbol = symbol,
                        TickOn = new DateTime(reader.getDate(i).Ticks, 
                            DateTimeKind.Utc).ToEstFromUtc(),
                        BidRate = reader.getBid(i).ToRoundedRate(symbol),
                        AskRate = reader.getAsk(i).ToRoundedRate(symbol)
                    };

                    ticks.Add(tick);
                }
            }

            return ticks;
        }
    }
}
