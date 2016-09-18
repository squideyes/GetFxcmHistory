// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using System;

namespace GetFxcmHistory.FXCM
{
    public static partial class Extenders
    {
        private const string EST_TZNAME = "Eastern Standard Time";
        private const string UTC_TZNAME = "UTC";

        private static readonly TimeZoneInfo estTzi;
        private static readonly TimeZoneInfo utcTzi;

        static Extenders()
        {
            estTzi = TimeZoneInfo.FindSystemTimeZoneById(EST_TZNAME);
            utcTzi = TimeZoneInfo.FindSystemTimeZoneById(UTC_TZNAME);
        }

        public static DateTime ToEstFromUtc(this DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                throw new ArgumentOutOfRangeException("dateTime.Kind");

            return TimeZoneInfo.ConvertTime(dateTime, estTzi);
        }

        public static DateTime ToUtcFromEst(this DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Unspecified)
                throw new ArgumentOutOfRangeException("dateTime.Kind");

            return TimeZoneInfo.ConvertTime(dateTime, estTzi, utcTzi);
        }

        public static string ToDateTimeText(this DateTime value) =>
            value.ToString("MM/dd/yyyy HH:mm:ss.fff");

        public static DateTime IgnoreMilliseconds(this DateTime value) =>
            value.AddMilliseconds(-value.Millisecond);
    }
}