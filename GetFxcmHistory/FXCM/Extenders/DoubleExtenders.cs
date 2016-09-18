// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using System;

namespace GetFxcmHistory.FXCM
{
    public static partial class Extenders
    {
        public static double ToRoundedRate(this double value, Symbol symbol) =>
            Math.Round(value, (symbol == Symbol.USDJPY) ? 3 : 5);

        public static string ToRateText(this double value, Symbol symbol) =>
            value.ToString(symbol == Symbol.USDJPY ? "N3" : "N5");
    }
}
