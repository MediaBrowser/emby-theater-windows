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
