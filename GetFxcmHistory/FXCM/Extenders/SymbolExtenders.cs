// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

namespace GetFxcmHistory.FXCM
{
    public static class SymbolExtenders
    {
        public static string ToInstrument(this Symbol symbol)
        {
            var s = symbol.ToString();

            return s.Substring(0, 3) + "/" + s.Substring(3);
        }
    }
}
