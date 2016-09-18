// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using GetFxcmHistory.FXCM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace GetFxcmHistory
{
    class Program
    {
        private class SpreadInfo
        {
            public SpreadInfo(Symbol symbol)
            {
                Symbol = symbol;
            }

            public Symbol Symbol { get; }

            public int Count { get; set; }
            public double Total { get; set; }

            public double AvgSpread
            {
                get
                {
                    return Math.Round((Total / Count) * 
                        (Symbol == Symbol.USDJPY ? 100.0 : 10000.0), 2);
                }
            }
        }

        static void Main(string[] args)
        {
            Console.SetWindowSize(100, 25);

            try
            {
                var tickSets = GetTickSets();

                FetchTickSets(tickSets);

                Console.WriteLine();

                GetTickSetStats(tickSets.Select(
                    ts => ts.Symbol).Distinct().ToList());
            }
            catch (Exception error)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Error: " + error.Message);
            }

            Console.WriteLine();
            Console.Write("Press any key to terminate the program...");

            Console.ReadKey(true);
        }

        private static void GetTickSetStats(List<Symbol> symbols)
        {
            using (var writer = new StreamWriter("Spreads.txt"))
            {
                foreach (var symbol in symbols)
                {
                    var folder = TickSet.GetSymbolPath(
                        Properties.Settings.Default.BasePath, symbol);

                    foreach (var fileName in Directory.GetFiles(
                        folder, "*.csv", SearchOption.AllDirectories))
                    {
                        var tickSet = TickSet.FromFile(fileName, true);

                        var totals = new Dictionary<int, SpreadInfo>();

                        for (int hour = 0; hour <= 23; hour++)
                            totals.Add(hour, new SpreadInfo(symbol));

                        foreach (var tick in tickSet)
                        {
                            totals[tick.TickOn.Hour].Count++;
                            totals[tick.TickOn.Hour].Total += tick.Spread;
                        }

                        for (int hour = 0; hour <= 23; hour++)
                        {
                            var dateTime = tickSet.Date.AddHours(hour);

                            writer.WriteLine(
                                $"{symbol},{dateTime:MM/dd/yyyy HH:mm:ss},{totals[hour].AvgSpread:N2}");
                        }

                        for (int hour = 0; hour <= 23; hour++)
                        {
                            var sb = new StringBuilder();

                            sb.Append(tickSet);
                            sb.Append($" - Hour: {hour:00}, Avg. Spread in PIPs: ");
                            sb.Append(totals[hour].AvgSpread.ToString("N2"));

                            Console.WriteLine(sb);
                        }

                        Console.WriteLine();
                    }
                }
            }
        }

        private static void FetchTickSets(List<TickSet> tickSets)
        {
            var cts = new CancellationTokenSource();

            var fetchBlock = new ActionBlock<TickSet>(
                tickSet =>
                {
                    if (tickSet.Exists(Properties.Settings.Default.BasePath))
                    {
                        Console.WriteLine($"[{tickSet}] Skipped");

                        return;
                    }

                    var fetcher = new Fetcher(Properties.Settings.Default.UserName,
                        Properties.Settings.Default.Password);

                    fetcher.OnStatus += (s, e) =>
                        Console.WriteLine($"[{tickSet}] Status: {e.Status}");

                    fetcher.OnError += (s, e) =>
                        Console.WriteLine($"[{tickSet}] Error: {e.Message}");

                    fetcher.OnProgress += (s, e) =>
                    {
                        var sb = new StringBuilder();

                        sb.Append($"[{tickSet}] Fetched Block {e.BlockId:000}, ");
                        sb.Append(e.MaxTickOn.ToString("HH:mm:ss.fff"));
                        sb.Append(" to ");
                        sb.Append(e.MinTickOn.ToString("HH:mm:ss.fff"));
                        sb.Append($", {e.TickCount:N0} Ticks");

                        Console.WriteLine(sb.ToString());
                    };

                    fetcher.OnTickSet += (s, e) => e.TickSet.Save(
                        Properties.Settings.Default.BasePath);

                    fetcher.Fetch(tickSet.Symbol, tickSet.Date);
                },
                new ExecutionDataflowBlockOptions()
                {
                    CancellationToken = cts.Token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                });

            tickSets.ForEach(tickSet => fetchBlock.Post(tickSet));

            fetchBlock.Complete();

            fetchBlock.Completion.Wait();
        }

        private static List<TickSet> GetTickSets()
        {
            var tickSets = new List<TickSet>();

            var doc = XDocument.Parse(Properties.Resources.TickSetsToFetch);

            var q = from ts in doc.Element("tickSetsToFetch").Elements("tickSet")
                    select new
                    {
                        Symbol = (Symbol)Enum.Parse(typeof(Symbol),
                            ts.Attribute("symbol").Value, true),
                        Date = (DateTime)ts.Attribute("date")
                    };

            return q.Select(ts => new TickSet(ts.Symbol, ts.Date)).ToList();
        }
    }
}