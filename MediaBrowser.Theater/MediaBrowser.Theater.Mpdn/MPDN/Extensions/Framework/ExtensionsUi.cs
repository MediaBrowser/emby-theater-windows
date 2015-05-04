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
using System.Windows.Forms;

namespace Mpdn
{
    namespace Config
    {
        namespace Internal
        {
            public class RenderScripts
            {
                // No implementation - we are only using this class as our folder name to pass into ScriptSettingsBase
            }

            public class PlayerExtensions
            {
                // No implementation - we are only using this class as our folder name to pass into ScriptSettingsBase
            }
        }

        public abstract class ScriptSettings<TSettings> 
            : ScriptSettingsBase<Internal.RenderScripts, TSettings> where TSettings : class, new()
        {
            protected ScriptSettings()
            {
            }

            protected ScriptSettings(TSettings settings)
                : base(settings)
            {
            }
        }

        public class NoSettings { }

        public interface IScriptConfigDialog<in TSettings> : IDisposable
            where TSettings : class, new()
        {
            void Setup(TSettings settings);
            DialogResult ShowDialog(IWin32Window owner);
        }

        public class ScriptConfigDialog<TSettings> : Form, IScriptConfigDialog<TSettings>
            where TSettings : class, new()
        {
            protected TSettings Settings { get; private set; }

            public virtual void Setup(TSettings settings)
            {
                Settings = settings;

                LoadSettings();
            }

            protected virtual void LoadSettings()
            {
                // This needs to be overriden
                throw new NotImplementedException();
            }

            protected virtual void SaveSettings()
            {
                // This needs to be overriden
                throw new NotImplementedException();
            }

            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                base.OnFormClosed(e);

                if (DialogResult != DialogResult.OK)
                    return;

                SaveSettings();
            }
        }

        public abstract class ExtensionUi<TExtensionClass, TSettings, TDialog> : IExtensionUi
            where TSettings : class, new()
            where TDialog : IScriptConfigDialog<TSettings>, new()
        {
            protected virtual string ConfigFileName { get { return this.GetType().Name; } }

            public abstract ExtensionUiDescriptor Descriptor { get; }

            #region Implementation

            private Config ScriptConfig { get; set; }

            public TSettings Settings
            {
                get 
                {
                    if (ScriptConfig == null) 
                        ScriptConfig = new Config(new TSettings());

                    return ScriptConfig.Config; 
                }
                set { ScriptConfig = new Config(value); }
            }

            public bool HasConfigDialog()
            {
                return !(typeof(TDialog).IsAssignableFrom(typeof(ScriptConfigDialog<TSettings>)));
            }

            public virtual void Initialize()
            {
                ScriptConfig = new Config(ConfigFileName);
            }

            public virtual void Destroy()
            {
                ScriptConfig.Save();
            }

            public virtual bool ShowConfigDialog(IWin32Window owner)
            {
                using (var dialog = new TDialog())
                {
                    dialog.Setup(ScriptConfig.Config);
                    if (dialog.ShowDialog(owner) != DialogResult.OK)
                        return false;

                    ScriptConfig.Save();
                    return true;
                }
            }

            #endregion

            #region ScriptSettings Class

            public class Config : ScriptSettingsBase<TExtensionClass, TSettings>
            {
                private readonly string m_ConfigName;

                public Config(string configName)
                {
                    m_ConfigName = configName;
                    Load();
                }

                public Config(TSettings settings)
                    : base(settings)
                {
                }

                protected override string ScriptConfigFileName
                {
                    get { return string.Format("{0}.config", m_ConfigName); }
                }
            }

            #endregion
        }
    }
}
