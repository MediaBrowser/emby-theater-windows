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

        public class ScriptConfigDialog<TSettings> : Form
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
            where TDialog : ScriptConfigDialog<TSettings>, new()
        {
            protected virtual string ConfigFileName { get { return this.GetType().Name; } }

            public abstract ExtensionUiDescriptor Descriptor { get; }

            #region Implementation

            private Config ScriptConfig { get; set; }

            protected TSettings Settings
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