using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Implementations.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System;

namespace MediaBrowser.UI.Configuration
{
    public class UIConfigurationManager : BaseConfigurationManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UIConfigurationManager" /> class.
        /// </summary>
        /// <param name="applicationPaths">The application paths.</param>
        /// <param name="logManager">The log manager.</param>
        /// <param name="xmlSerializer">The XML serializer.</param>
        public UIConfigurationManager(IApplicationPaths applicationPaths, ILogManager logManager, IXmlSerializer xmlSerializer)
            : base(applicationPaths, logManager, xmlSerializer)
        {
        }

        /// <summary>
        /// Gets the type of the configuration.
        /// </summary>
        /// <value>The type of the configuration.</value>
        protected override Type ConfigurationType
        {
            get { return typeof(UIApplicationConfiguration); }
        }

        /// <summary>
        /// Gets the application paths.
        /// </summary>
        /// <value>The application paths.</value>
        public UIApplicationPaths ApplicationPaths
        {
            get { return (UIApplicationPaths)CommonApplicationPaths; }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public UIApplicationConfiguration Configuration
        {
            get { return (UIApplicationConfiguration)CommonConfiguration; }
        }
    }
}
