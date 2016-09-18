// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using System;
using System.Text;

namespace GetFxcmHistory.FXCM
{
    public class Tick : IEquatable<Tick>
    {
        public Symbol Symbol { get; set; }
        public DateTime TickOn { get; set; }
        public double BidRate { get; set; }
        public double AskRate { get; set; }

        public double Spread => 
            (AskRate - BidRate).ToRoundedRate(Symbol);

        public bool Equals(Tick other)
        {
            return Symbol.Equals(other.Symbol)
                && TickOn.Equals(other.TickOn)
                && BidRate.Equals(other.BidRate)
                && AskRate.Equals(other.AskRate);
        }

        public override int GetHashCode()
        {
            return Symbol.GetHashCode()
                ^ TickOn.GetHashCode()
                ^ BidRate.GetHashCode()
                ^ AskRate.GetHashCode();
        }

        public override bool Equals(object other)
        {
            var tick = other as Tick;

            if (tick == null)
                return false;

            return Equals(tick);
        }

        public static bool operator
            ==(Tick a, Tick b) => a.Equals(b);

        public static bool operator
            !=(Tick a, Tick b) => !a.Equals(b);

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Symbol);
            sb.Append(',');
            sb.Append(TickOn.ToDateTimeText());
            sb.Append(',');
            sb.Append(BidRate.ToRateText(Symbol));
            sb.Append(',');
            sb.Append(AskRate.ToRateText(Symbol));

            return sb.ToString();
        }

        public string ToCsvString() => ToString();
    }
}
