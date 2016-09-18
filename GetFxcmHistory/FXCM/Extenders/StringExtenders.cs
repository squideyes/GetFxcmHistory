// Copyright (C) SquidEyes, LLC. - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and Confidential
// Written by Louis S. Berman <louis@squideyes.com>, 9/18/2016

using System;
using System.IO;
using System.Text;

namespace GetFxcmHistory.FXCM
{
    public static partial class Extenders
    {
        public static string ToSingleLine(this string value, string delimiter = "; ")
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var reader = new StringReader(value);

            var sb = new StringBuilder();

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (sb.Length > 0)
                    sb.Append(delimiter);

                sb.Append(line.Trim());
            }

            return sb.ToString();
        }

        public static bool IsFolderName(this string value, bool mustBeRooted = true)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (value.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                return false;

            try
            {
                new DirectoryInfo(value);

                if (!mustBeRooted)
                    return true;
                else
                    return Path.IsPathRooted(value);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (PathTooLongException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
    }
}