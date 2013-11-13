using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Events;
using MediaBrowser.Common.Implementations.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Theater.Interfaces.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MediaBrowser.Theater.Implementations.Configuration
{
    public class ConfigurationManager : BaseConfigurationManager, ITheaterConfigurationManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationManager" /> class.
        /// </summary>
        /// <param name="applicationPaths">The application paths.</param>
        /// <param name="logManager">The log manager.</param>
        /// <param name="xmlSerializer">The XML serializer.</param>
        public ConfigurationManager(IApplicationPaths applicationPaths, ILogManager logManager, IXmlSerializer xmlSerializer)
            : base(applicationPaths, logManager, xmlSerializer)
        {
        }

        /// <summary>
        /// Gets the type of the configuration.
        /// </summary>
        /// <value>The type of the configuration.</value>
        protected override Type ConfigurationType
        {
            get { return typeof(ApplicationConfiguration); }
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
        public ApplicationConfiguration Configuration
        {
            get { return (ApplicationConfiguration)CommonConfiguration; }
        }

        private string GetConfigPath(string userId)
        {
            return Path.Combine(ApplicationPaths.ConfigurationDirectoryPath, userId + ".xml");
        }

        public UserTheaterConfiguration GetUserTheaterConfiguration(string userId)
        {
            var path = GetConfigPath(userId);

            try
            {
                return (UserTheaterConfiguration)XmlSerializer.DeserializeFromFile(typeof(UserTheaterConfiguration), path);
            }
            catch (DirectoryNotFoundException)
            {
                return new UserTheaterConfiguration();
            }
            catch (FileNotFoundException)
            {
                return new UserTheaterConfiguration();
            }
        }

        public Task UpdateUserTheaterConfiguration(string userId, UserTheaterConfiguration configuration)
        {
            return Task.Run(() =>
            {
                var path = GetConfigPath(userId);

                XmlSerializer.SerializeToFile(configuration, path);

                EventHelper.FireEventIfNotNull(UserConfigurationUpdated, this, new UserConfigurationUpdatedEventArgs { UserId = userId, Configuration = configuration }, Logger);
            });
        }

        public event EventHandler<UserConfigurationUpdatedEventArgs> UserConfigurationUpdated;
    }
}
