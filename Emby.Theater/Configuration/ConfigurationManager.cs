using Emby.Theater.App;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System;
using Emby.Theater.AppBase;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;

namespace Emby.Theater.Configuration
{
    public class ConfigurationManager : BaseConfigurationManager, ITheaterConfigurationManager
    {
        public ConfigurationManager(IApplicationPaths applicationPaths, ILogManager logManager, IXmlSerializer xmlSerializer, IFileSystem fileSystem)
            : base(applicationPaths, logManager, xmlSerializer, fileSystem)
        {
        }

        /// <summary>
        /// Gets the type of the configuration.
        /// </summary>
        /// <value>The type of the configuration.</value>
        protected override Type ConfigurationType
        {
            get { return typeof(BaseApplicationConfiguration); }
        }

        /// <summary>
        /// Gets the application paths.
        /// </summary>
        /// <value>The application paths.</value>
        public ApplicationPaths ApplicationPaths
        {
            get { return (ApplicationPaths)CommonApplicationPaths; }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public BaseApplicationConfiguration Configuration
        {
            get { return (BaseApplicationConfiguration)CommonConfiguration; }
        }
    }
}
