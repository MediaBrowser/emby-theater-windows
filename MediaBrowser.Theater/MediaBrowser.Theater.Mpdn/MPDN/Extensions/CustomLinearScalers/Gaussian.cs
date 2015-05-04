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
