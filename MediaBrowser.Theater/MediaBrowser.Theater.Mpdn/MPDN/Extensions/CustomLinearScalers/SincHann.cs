using System;

namespace Mpdn.CustomLinearScaler
{
    namespace Example
    {
        public class SincHann : Sinc
        {
            public override Guid Guid
            {
                get { return new Guid("5BF9892B-98B0-4C00-AA4C-082F6ADB7555"); }
            }

            public override string WindowName
            {
                get { return "Hann"; }
            }

            public override double GetWindowWeight(double x, double radius)
            {
                var f = x * Math.PI;

                var cosine = Math.Cos(f / radius);
                return 0.5 + 0.5 * cosine;
            }
        }
    }
}
