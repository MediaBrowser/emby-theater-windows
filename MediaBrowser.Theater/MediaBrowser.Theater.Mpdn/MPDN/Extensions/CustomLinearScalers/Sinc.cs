// This file is a part of MPDN Extensions.
// https://github.com/zachsaw/MPDN_Extensions
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// 
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
