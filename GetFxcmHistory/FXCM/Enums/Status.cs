// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using fxcore2;

namespace GetFxcmHistory.FXCM
{
    public enum Status
    {
        Connected = O2GSessionStatusCode.Connected,
        Connecting = O2GSessionStatusCode.Connecting,
        Disconnecting = O2GSessionStatusCode.Disconnecting,
        Disconnected = O2GSessionStatusCode.Disconnected,
        PriceSessionReconnecting = O2GSessionStatusCode.PriceSessionReconnecting,
        Reconnecting = O2GSessionStatusCode.Reconnecting,
        SessionLost = O2GSessionStatusCode.SessionLost,
        TradingSessionRequested = O2GSessionStatusCode.TradingSessionRequested,
        Unknown = O2GSessionStatusCode.Unknown
    }
}
