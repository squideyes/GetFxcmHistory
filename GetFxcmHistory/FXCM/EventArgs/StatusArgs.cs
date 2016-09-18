// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using System;

namespace GetFxcmHistory.FXCM
{
    public class StatusArgs : EventArgs
    {
        internal StatusArgs(Status status)
        {
            Status = status;
        }

        public Status Status { get; }
    }
}
