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
