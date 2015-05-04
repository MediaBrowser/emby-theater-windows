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
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Mpdn
{
    public static class DisposeHelper
    {
        public static void Dispose(object obj)
        {
            var disposable = obj as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        public static void Dispose<T>(ref T obj) where T : class, IDisposable
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = default(T);
            }
        }
    }

    public static class PathHelper
    {
        public static string GetDirectoryName(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            return Path.GetDirectoryName(path) ?? Path.GetPathRoot(path);
        }
    }

    public static class EnumHelpers
    {
        public static string ToDescription(this Enum en)
        {
            var type = en.GetType();
            var enumString = en.ToString();

            var memInfo = type.GetMember(enumString);

            if (memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof (DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute) attrs[0]).Description;
                }
            }

            return enumString;
        }

        public static string[] GetDescriptions<T>()
        {
            return Enum.GetValues(typeof (T)).Cast<Enum>().Select(val => val.ToDescription()).ToArray();
        }
    }

    public static class RenderQualityHelpers
    {
        public static TextureFormat GetTextureFormat(this RenderQuality quality)
        {
            switch (quality)
            {
                case RenderQuality.MaxPerformance:
                    return TextureFormat.Unorm8;
                case RenderQuality.Performance:
                    return TextureFormat.Float16;
                case RenderQuality.Quality:
                    return TextureFormat.Unorm16;
                case RenderQuality.MaxQuality:
                    return TextureFormat.Float32;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
