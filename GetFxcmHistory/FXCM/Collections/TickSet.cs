// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GetFxcmHistory.FXCM
{
    public class TickSet : IEnumerable<Tick>
    {
        private List<Tick> ticks = new List<Tick>();

        public TickSet(Symbol symbol, DateTime date)
        {
            if (!symbol.IsDefinedEnum())
                throw new ArgumentOutOfRangeException(nameof(symbol));

            if (date.Kind != DateTimeKind.Unspecified)
                throw new ArgumentOutOfRangeException(nameof(date));

            if (date.TimeOfDay != TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(date));

            Symbol = symbol;
            Date = date;
        }

        public Symbol Symbol { get; }
        public DateTime Date { get; }

        public string FileName =>
            $"FXCM_FCAPI_{Symbol}_{Date:yyyy_MM_dd}_00_24H_EST.csv";

        public string GetSaveTo(string basePath)
        {
            if (!basePath.IsFolderName(false))
                throw new ArgumentOutOfRangeException(nameof(basePath));

            return Path.Combine(GetSymbolPath(basePath, Symbol), 
                Date.Year.ToString(), FileName);
        }

        public bool Exists(string basePath) => File.Exists(GetSaveTo(basePath));

        public void Load(IEnumerable<Tick> ticks)
        {
            if (ticks == null)
                throw new ArgumentNullException(nameof(ticks));

            this.ticks.Clear();

            this.ticks.AddRange(ticks);
        }

        public void Load(string fileName)
        {
            ticks.Clear();

            using (var reader = new StreamReader(fileName))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split(',');

                    var symbol = (Symbol)Enum.Parse(typeof(Symbol), fields[0], true);

                    if (symbol != Symbol)
                        throw new ArgumentOutOfRangeException("line:Symbol");

                    ticks.Add(new Tick()
                    {
                        Symbol= symbol,
                        TickOn = DateTime.ParseExact(
                            fields[1], "MM/dd/yyyy HH:mm:ss.fff", null),
                        BidRate = double.Parse(fields[2]).ToRoundedRate(symbol),
                        AskRate = double.Parse(fields[3]).ToRoundedRate(symbol),
                    });
                }
            }
        }

        public void Save(string basePath)
        {
            if (!basePath.IsFolderName(false))
                throw new ArgumentOutOfRangeException(nameof(basePath));

            var saveTo = GetSaveTo(basePath);

            var folder = Path.GetDirectoryName(saveTo);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            using (var writer = new StreamWriter(saveTo))
            {
                foreach (var tick in ticks)
                    writer.WriteLine(tick.ToCsvString());
            }
        }

        public int Count => ticks.Count;

        public Tick this[int index] => ticks[index];

        public IEnumerator<Tick> GetEnumerator() => ticks.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => $"{Symbol} {Date:MM/dd/yyyy}";

        public static string GetSymbolPath(string basePath, Symbol symbol) =>
            Path.Combine(basePath, "FXCM", "FCAPI", "CSV", symbol.ToString());

        public static TickSet FromFile(string fileName, bool load = false)
        {
            var nameOnly = Path.GetFileName(fileName);

            if (!nameOnly.StartsWith("FXCM_FCAPI_"))
                throw new ArgumentOutOfRangeException(nameof(fileName));

            if (!nameOnly.EndsWith("_00_24H_EST.csv"))
                throw new ArgumentOutOfRangeException(nameof(fileName));

            var fields = nameOnly.Split('_');

            var symbol = (Symbol)Enum.Parse(typeof(Symbol), fields[2]);

            var year = int.Parse(fields[3]);
            var month = int.Parse(fields[4]);
            var day = int.Parse(fields[5]);

            var date = new DateTime(year, month, day);

            var tickSet = new TickSet(symbol, date);

            if (load)
                tickSet.Load(fileName);

            return tickSet;
        }
    }
}
