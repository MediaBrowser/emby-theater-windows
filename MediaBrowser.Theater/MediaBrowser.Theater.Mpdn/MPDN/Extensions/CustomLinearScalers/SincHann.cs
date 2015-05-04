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
