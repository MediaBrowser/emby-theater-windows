using System;

namespace Mpdn.CustomLinearScaler
{
    namespace Example
    {
        public class Gaussian : ICustomLinearScaler
        {
            public Guid Guid
            {
                get { return new Guid("647351FF-7FEC-4EAB-86C7-CE1BEF43EFD4"); }
            }

            public string Name
            {
                get { return "Gaussian"; }
            }

            public bool AllowDeRing
            {
                get { return false; }
            }

            public ScalerTaps MaxTapCount
            {
                get { return ScalerTaps.Eight; }
            }

            public float GetWeight(float n, int width)
            {
                return (float) GaussianKernel(n, width / 2);
            }

            private static double GaussianKernel(double x, double radius)
            {
                var sigma = radius / 4;
                return Math.Exp(-(x*x/(2*sigma*sigma)));
            }
        }
    }
}
