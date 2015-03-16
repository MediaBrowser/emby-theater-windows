using System;

namespace Mpdn.CustomLinearScaler
{
    namespace Example
    {
        public class SincCosine : Sinc
        {
            public override Guid Guid
            {
                get { return new Guid("687A62CC-8A73-4511-BB1F-158F61B8CDA1"); }
            }

            public override string WindowName
            {
                get { return "Cosine"; }
            }

            public override double GetWindowWeight(double x, double radius)
            {
                var f = x * Math.PI / 2;
                return Math.Cos(f / radius);
            }
        }
    }
}
