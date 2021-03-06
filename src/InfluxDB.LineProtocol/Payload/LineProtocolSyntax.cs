﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace InfluxDB.LineProtocol.Payload
{
    class LineProtocolSyntax
    {
        static readonly DateTime Origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static readonly Dictionary<Type, Func<object, string>> Formatters = new Dictionary<Type, Func<object, string>>
        {
            { typeof(sbyte), FormatInteger },
            { typeof(byte), FormatInteger },
            { typeof(short), FormatInteger },
            { typeof(ushort), FormatInteger },
            { typeof(int), FormatInteger },
            { typeof(uint), FormatInteger },
            { typeof(long), FormatInteger },
            { typeof(ulong), FormatInteger },
            { typeof(float), FormatFloat },
            { typeof(double), FormatFloat },
            { typeof(decimal), FormatFloat },
            { typeof(bool), FormatBoolean },
            { typeof(TimeSpan), FormatTimespan }
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EscapeName(string nameOrKey, TextWriter textWriter)
        {
            if (nameOrKey == null) throw new ArgumentNullException(nameof(nameOrKey));

            for (var i = 0; i < nameOrKey.Length; i++)
            {
                switch (nameOrKey[i])
                {
                    case '=':
                        textWriter.Write("\\=");
                        break;
                    case ' ':
                        textWriter.Write("\\ ");
                        break;
                    case ',':
                        textWriter.Write("\\,");
                        break;
                    default:
                        textWriter.Write(nameOrKey[i]);
                        break;
                }
            }
        }

        public static string FormatValue(object value)
        {
            var v = value ?? "";
            Func<object, string> format;
            if (Formatters.TryGetValue(v.GetType(), out format))
                return format(v);
            return FormatString(v.ToString());
        }

        static string FormatInteger(object i)
        {
            return ((IFormattable)i).ToString(null, CultureInfo.InvariantCulture) + "i";
        }

        static string FormatFloat(object f)
        {
            return ((IFormattable)f).ToString(null, CultureInfo.InvariantCulture);
        }

        static string FormatTimespan(object ts)
        {
            return ((TimeSpan)ts).TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }

        static string FormatBoolean(object b)
        {
            return ((bool)b) ? "t" : "f";
        }

        static string FormatString(string s)
        {
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatTimestamp(DateTime utcTimestamp)
        {
            var t = utcTimestamp - Origin;
            return (t.Ticks * 100L).ToString(NumberFormatInfo.InvariantInfo);
        }
    }
}
