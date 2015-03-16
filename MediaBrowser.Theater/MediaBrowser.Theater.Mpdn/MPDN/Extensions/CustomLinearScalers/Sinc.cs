using System;

namespace Mpdn.CustomLinearScaler
{
    public abstract class Sinc : ICustomLinearScaler
    {
        public abstract Guid Guid { get; }

        public abstract string WindowName { get; }

        public virtual string Name
        {
            get { return string.Format("Sinc-{0}", WindowName); }
        }

        public virtual bool AllowDeRing
        {
            get { return true; }
        }

        public virtual ScalerTaps MaxTapCount
        {
            get { return ScalerTaps.Sixteen; }
        }

        public virtual float GetWeight(float x, int width)
        {
            var n = Math.Max(Math.Abs(x), 1e-8);
            return (float) (CalculateSinc(n)*GetWindowWeight(n, width/2.0));
        }

        public abstract double GetWindowWeight(double x, double radius);

        protected static double CalculateSinc(double x)
        {
            var f = x*Math.PI;
            return Math.Sin(f)/f;
        }
    }
}
