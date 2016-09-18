// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using fxcore2;
using System;
using System.Threading;

namespace GetFxcmHistory.FXCM
{
    public class StatusListener : IO2GSessionStatus
    {
        private O2GSession session;
        private EventWaitHandle syncSessionEvent;

        public event EventHandler<StatusArgs> OnStatus;
        public event EventHandler<ErrorArgs> OnError;

        public StatusListener(O2GSession session)
        {
            this.session = session;

            Reset();

            syncSessionEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        public bool IsConnected { get; private set; }

        public void Reset() => IsConnected = false;

        public bool WaitEvents() => syncSessionEvent.WaitOne(30000);

        public void onSessionStatusChanged(O2GSessionStatusCode status)
        {
            OnStatus?.Invoke(this, new StatusArgs((Status)(int)status));

            switch (status)
            {
                case O2GSessionStatusCode.Connected:
                    IsConnected = true;
                    syncSessionEvent.Set();
                    break;
                case O2GSessionStatusCode.Disconnected:
                    IsConnected = false;
                    syncSessionEvent.Set();
                    break;
            }
        }

        public void onLoginFailed(string errorMessage) =>
            OnError?.Invoke(this, new ErrorArgs(ErrorKind.Login, errorMessage));
    }
}
