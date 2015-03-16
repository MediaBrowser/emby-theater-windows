using System;

namespace Mpdn.CustomLinearScaler
{
    namespace Example
    {
        public class SincHamming : Sinc
        {
            public override Guid Guid
            {
                get { return new Guid("DC789FC7-FED2-4741-8D75-8D8888A62AD0"); }
            }

            public override string WindowName
            {
                get { return "Hamming"; }
            }

            public override double GetWindowWeight(double x, double radius)
            {
                var f = x * Math.PI;

                var cosine = Math.Cos(f / radius);
                return 0.54 + 0.46 * cosine;
            }
        }
    }
}
