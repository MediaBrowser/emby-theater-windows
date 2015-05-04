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
