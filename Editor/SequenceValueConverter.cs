#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using System.Globalization;
using UnityEngine;

namespace MartinCalander.OdinSequence.Editor
{
    internal static class SequenceValueConverter
    {
        public static bool TryReadNumber(object value, out double number)
        {
            number = 0d;
            if (value == null)
                return false;

            try
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                        return SequenceStripLayout.IsFinite(number);
                    default:
                        return false;
                }
            }
            catch (Exception exception) when (exception is InvalidCastException ||
                                               exception is FormatException ||
                                               exception is OverflowException)
            {
                return false;
            }
        }

        public static bool TryConvertNumber(double value, Type destinationType, out object converted)
        {
            converted = null;
            if (!SequenceStripLayout.IsFinite(value) || destinationType == null)
                return false;

            Type type = Nullable.GetUnderlyingType(destinationType) ?? destinationType;

            try
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.SByte:
                        return TryConvertInteger(value, sbyte.MinValue, sbyte.MaxValue,
                            rounded => checked((sbyte)rounded), out converted);
                    case TypeCode.Byte:
                        return TryConvertInteger(value, byte.MinValue, byte.MaxValue,
                            rounded => checked((byte)rounded), out converted);
                    case TypeCode.Int16:
                        return TryConvertInteger(value, short.MinValue, short.MaxValue,
                            rounded => checked((short)rounded), out converted);
                    case TypeCode.UInt16:
                        return TryConvertInteger(value, ushort.MinValue, ushort.MaxValue,
                            rounded => checked((ushort)rounded), out converted);
                    case TypeCode.Int32:
                        return TryConvertInteger(value, int.MinValue, int.MaxValue,
                            rounded => checked((int)rounded), out converted);
                    case TypeCode.UInt32:
                        return TryConvertInteger(value, uint.MinValue, uint.MaxValue,
                            rounded => checked((uint)rounded), out converted);
                    case TypeCode.Int64:
                        return TryConvertInteger(value, long.MinValue, long.MaxValue,
                            rounded => checked((long)rounded), out converted);
                    case TypeCode.UInt64:
                        return TryConvertInteger(value, ulong.MinValue, ulong.MaxValue,
                            rounded => checked((ulong)rounded), out converted);
                    case TypeCode.Single:
                    {
                        float single = (float)value;
                        if (float.IsNaN(single) || float.IsInfinity(single))
                            return false;
                        converted = single;
                        return true;
                    }
                    case TypeCode.Double:
                        converted = value;
                        return true;
                    case TypeCode.Decimal:
                        converted = (decimal)value;
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception exception) when (exception is InvalidCastException ||
                                               exception is OverflowException)
            {
                return false;
            }
        }

        public static double ResolveMinimumDuration(double configuredMinimum, Type destinationType)
        {
            const double fallbackMinimum = 0.000001d;
            double minimum = SequenceStripLayout.IsFinite(configuredMinimum)
                ? Math.Max(fallbackMinimum, configuredMinimum)
                : fallbackMinimum;

            if (destinationType == null)
                return minimum;

            Type type = Nullable.GetUnderlyingType(destinationType) ?? destinationType;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return Math.Max(1d, Math.Ceiling(minimum));
                default:
                    return minimum;
            }
        }

        public static bool TryReadLane(object value, out int lane)
        {
            lane = 0;
            if (!TryReadNumber(value, out double number))
                return false;

            double rounded = Math.Round(number, MidpointRounding.AwayFromZero);
            if (rounded < int.MinValue || rounded > int.MaxValue)
                return false;

            lane = (int)rounded;
            return true;
        }

        public static bool TryReadColor(object value, out Color color)
        {
            if (value is Color colorValue)
            {
                color = colorValue;
                return true;
            }

            if (value is Color32 color32Value)
            {
                color = color32Value;
                return true;
            }

            color = default;
            return false;
        }

        public static string ReadLabel(object value, string fallback)
        {
            if (value == null)
                return fallback;

            if (value is string text)
                return string.IsNullOrEmpty(text) ? fallback : text;

            try
            {
                string convertedText = Convert.ToString(value, CultureInfo.InvariantCulture);
                return string.IsNullOrEmpty(convertedText) ? fallback : convertedText;
            }
            catch (Exception exception) when (exception is InvalidCastException || exception is FormatException)
            {
                return fallback;
            }
        }

        private static bool TryConvertInteger(
            double value,
            double minimum,
            double maximum,
            Func<double, object> convert,
            out object converted)
        {
            converted = null;
            double rounded = Math.Round(value, MidpointRounding.AwayFromZero);
            if (rounded < minimum || rounded > maximum)
                return false;

            converted = convert(rounded);
            return true;
        }
    }
}
#endif
