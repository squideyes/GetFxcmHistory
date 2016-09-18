// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using System;

namespace GetFxcmHistory.FXCM
{
    public class ErrorArgs : EventArgs
    {
        internal ErrorArgs(ErrorKind kind, string message)
        {
            Kind = kind;
            Message = message.ToSingleLine();
        }

        public ErrorKind Kind { get; }
        public string Message { get; }
    }
}
