using System;

namespace Mpdn.CustomLinearScaler
{
    namespace Example
    {
        public class SincBlackman : Sinc
        {
            public override Guid Guid
            {
                get { return new Guid("E9B7ACD5-A53E-4984-8FFE-987DA89D4751"); }
            }

            public override string WindowName
            {
                get { return "Blackman"; }
            }

            public override double GetWindowWeight(double x, double radius)
            {
                var f = x*Math.PI;

                var cosine = Math.Cos(f/radius);
                return 0.34 + cosine*(0.5 + cosine*0.16);
            }
        }
    }
}
