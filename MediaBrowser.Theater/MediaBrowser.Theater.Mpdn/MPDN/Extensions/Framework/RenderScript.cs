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
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Mpdn.OpenCl;

namespace Mpdn.RenderScript
{
    public static class RenderScript
    {
        public static IRenderScriptUi Empty = new NullRenderScriptUi();

        public static bool IsEmpty(this IRenderScriptUi script)
        {
            return script is NullRenderScriptUi;
        }

        private class NullRenderScriptUi : IRenderScriptUi
        {
            public ExtensionUiDescriptor Descriptor
            {
                get
                {
                    return new ExtensionUiDescriptor
                    {
                        Guid = Guid.Empty,
                        Name = "None",
                        Description = "Do not use render script"
                    };
                }
            }

            public IRenderScript CreateRenderScript()
            {
                return null;
            }

            public void Initialize()
            {
            }

            public void Destroy()
            {
            }

            public bool HasConfigDialog()
            {
                return false;
            }

            public bool ShowConfigDialog(IWin32Window owner)
            {
                return false;
            }
        }
    }


    public static class ShaderCache
    {
        private static class InternalCache<T>
        where T : class
        {
            private static readonly Dictionary<string, ShaderWithDateTime> s_CompiledShaders =
                new Dictionary<string, ShaderWithDateTime>();

            public static T Add(string shaderPath, string key, Func<T> compileFunc)
            {
                var lastMod = File.GetLastWriteTimeUtc(shaderPath);

                ShaderWithDateTime result;
                if (s_CompiledShaders.TryGetValue(key, out result) &&
                    result.LastModified == lastMod)
                {
                    return result.Shader;
                }

                if (result != null)
                {
                    DisposeHelper.Dispose(result.Shader);
                    s_CompiledShaders.Remove(key);
                }

                var shader = compileFunc();
                s_CompiledShaders.Add(key, new ShaderWithDateTime(shader, lastMod));
                return shader;
            }

            public class ShaderWithDateTime
            {
                public T Shader { get; private set; }
                public DateTime LastModified { get; private set; }

                public ShaderWithDateTime(T shader, DateTime lastModified)
                {
                    Shader = shader;
                    LastModified = lastModified;
                }
            }
        }

        public static IShader CompileShader(string shaderFileName, string entryPoint = "main", string macroDefinitions = null)
        {
            return InternalCache<IShader>.Add(shaderFileName,
                String.Format("\"{0}\" /E {1} /D {2}", shaderFileName, entryPoint, macroDefinitions),
                () => Renderer.CompileShader(shaderFileName, entryPoint, macroDefinitions));
        }

        public static IShader11 CompileShader11(string shaderFileName, string profile, string entryPoint = "main", string macroDefinitions = null)
        {
            return InternalCache<IShader11>.Add(shaderFileName,
                String.Format("\"{0}\" /E {1} /T {2} /D {3}", shaderFileName, entryPoint, profile, macroDefinitions),
                () => Renderer.CompileShader11(shaderFileName, entryPoint, profile, macroDefinitions));
        }

        public static IKernel CompileClKernel(string sourceFileName, string entryPoint, string options = null)
        {
            return InternalCache<IKernel>.Add(sourceFileName,
                String.Format("\"{0}\" /E {1} /Opts {2}", sourceFileName, entryPoint, options),
                () => Renderer.CompileClKernel(sourceFileName, entryPoint, options));
        }

        public static IShader LoadShader(string shaderFileName)
        {
            return InternalCache<IShader>.Add(shaderFileName,
                shaderFileName,
                () => Renderer.LoadShader(shaderFileName));
        }

        public static IShader11 LoadShader11(string shaderFileName)
        {
            return InternalCache<IShader11>.Add(shaderFileName,
                shaderFileName,
                () => Renderer.LoadShader11(shaderFileName));
        }
    }
}