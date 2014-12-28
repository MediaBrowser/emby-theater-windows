using MediaBrowser.ApiInteraction.Data;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Sync;
using System.IO;
using System.Threading.Tasks;

namespace MediaBrowser.UI.Sync
{
    public class ItemRepository : IItemRepository
    {
        private readonly IApplicationPaths _appPaths;
        private readonly IJsonSerializer _json;

        public ItemRepository(IApplicationPaths appPaths, IJsonSerializer json)
        {
            _appPaths = appPaths;
            _json = json;
        }

        private string SyncRootPath
        {
            get { return Path.Combine(_appPaths.ProgramDataPath, "sync", "data"); }
        }

        public Task AddOrUpdate(LocalItem item)
        {
            var path = GetPath(item.UniqueId);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            _json.SerializeToFile(item, path);

            return Task.FromResult(true);
        }

        public Task<LocalItem> Get(string id)
        {
            return Task.FromResult(_json.DeserializeFromFile<LocalItem>(GetPath(id)));
        }

        private string GetPath(string id)
        {
            return Path.Combine(SyncRootPath, id + ".json");
        }
    }
}
