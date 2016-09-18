// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using fxcore2;
using System;
using System.Threading;

namespace GetFxcmHistory.FXCM
{
    internal class ResponseListener : IO2GResponseListener
    {
        public class RequestFailedArgs
        {
            public RequestFailedArgs(string errorMessage)
            {
                ErrorMessage = errorMessage;
            }

            public string ErrorMessage { get; }
        }

        public event EventHandler<RequestFailedArgs> OnRequestFailed;
        public event EventHandler OnNoTicks;

        private string requestId;
        private O2GResponse response;
        private EventWaitHandle syncResponseEvent;

        public ResponseListener(O2GSession session)
        {
            requestId = string.Empty;

            response = null;

            syncResponseEvent = new EventWaitHandle(
                false, EventResetMode.AutoReset);
        }

        public void SetRequestID(string requestId)
        {
            response = null;

            this.requestId = requestId;
        }

        public bool WaitEvents() => syncResponseEvent.WaitOne(30000);

        public O2GResponse GetResponse() => response;

        public void onRequestCompleted(string requestId, O2GResponse response)
        {
            if (this.requestId.Equals(response.RequestID))
            {
                this.response = response;

                syncResponseEvent.Set();
            }
        }

        public void onRequestFailed(string requestId, string errorMessage)
        {
            if (this.requestId.Equals(requestId))
            {
                response = null;

                if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = "There is no more data";

                if (errorMessage.StartsWith(
                    "Reason='unsupported scope', Description=No data found for symbolID="))
                {
                    OnNoTicks?.Invoke(this, EventArgs.Empty);
                }
                else
                { 
                    OnRequestFailed?.Invoke(this, new RequestFailedArgs(errorMessage));
                }

                syncResponseEvent.Set();
            }
        }

        public void onTablesUpdates(O2GResponse data)
        {
        }
    }
}
