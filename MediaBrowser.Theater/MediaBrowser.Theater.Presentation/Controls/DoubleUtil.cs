using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace MediaBrowser.Theater.Presentation.Controls
{
    internal static class DoubleUtil
    {
        internal const double DBL_EPSILON = 2.2204460492503131E-16;
        internal const float FLT_MIN = 1.17549435E-38f;

        public static bool AreClose(double value1, double value2)
        {
            if (value1.Equals(value2)) {
                return true;
            }
            double num = (Math.Abs(value1) + Math.Abs(value2) + 10.0)*2.2204460492503131E-16;
            double num2 = value1 - value2;
            return -num < num2 && num > num2;
        }

        public static bool LessThan(double value1, double value2)
        {
            return value1 < value2 && !AreClose(value1, value2);
        }

        public static bool GreaterThan(double value1, double value2)
        {
            return value1 > value2 && !AreClose(value1, value2);
        }

        public static bool LessThanOrClose(double value1, double value2)
        {
            return value1 < value2 || AreClose(value1, value2);
        }

        public static bool GreaterThanOrClose(double value1, double value2)
        {
            return value1 > value2 || AreClose(value1, value2);
        }

        public static bool IsOne(double value)
        {
            return Math.Abs(value - 1.0) < 2.2204460492503131E-15;
        }

        public static bool IsZero(double value)
        {
            return Math.Abs(value) < 2.2204460492503131E-15;
        }

        public static bool AreClose(Point point1, Point point2)
        {
            return AreClose(point1.X, point2.X) && AreClose(point1.Y, point2.Y);
        }

        public static bool AreClose(Size size1, Size size2)
        {
            return AreClose(size1.Width, size2.Width) && AreClose(size1.Height, size2.Height);
        }

        public static bool AreClose(Vector vector1, Vector vector2)
        {
            return AreClose(vector1.X, vector2.X) && AreClose(vector1.Y, vector2.Y);
        }

        public static bool AreClose(Rect rect1, Rect rect2)
        {
            if (rect1.IsEmpty) {
                return rect2.IsEmpty;
            }
            return !rect2.IsEmpty && AreClose(rect1.X, rect2.X) && AreClose(rect1.Y, rect2.Y) && AreClose(rect1.Height, rect2.Height) && AreClose(rect1.Width, rect2.Width);
        }

        public static bool IsBetweenZeroAndOne(double val)
        {
            return GreaterThanOrClose(val, 0.0) && LessThanOrClose(val, 1.0);
        }

        public static int DoubleToInt(double val)
        {
            if (0.0 >= val) {
                return (int) (val - 0.5);
            }
            return (int) (val + 0.5);
        }

        public static bool RectHasNaN(Rect r)
        {
            return IsNaN(r.X) || IsNaN(r.Y) || IsNaN(r.Height) || IsNaN(r.Width);
        }

        public static bool IsNaN(double value)
        {
            NanUnion nanUnion = default(NanUnion);
            nanUnion.DoubleValue = value;
            ulong num = nanUnion.UintValue & 18442240474082181120uL;
            ulong num2 = nanUnion.UintValue & 4503599627370495uL;
            return (num == 9218868437227405312uL || num == 18442240474082181120uL) && num2 != 0uL;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct NanUnion
        {
            [FieldOffset(0)] internal double DoubleValue;
            [FieldOffset(0)] internal readonly ulong UintValue;
        }
    }
}