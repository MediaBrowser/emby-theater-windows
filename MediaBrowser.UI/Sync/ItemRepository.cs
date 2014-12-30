using System.Linq;
using MediaBrowser.ApiInteraction.Data;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Sync;
using System.Collections.Generic;
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

        public Task<List<string>> GetServerItemIds(string serverId)
        {
            try
            {
                var list = new DirectoryInfo(SyncRootPath).EnumerateFiles("*.json", SearchOption.AllDirectories)
                    .Select(i => _json.DeserializeFromFile<LocalItem>(i.FullName))
                    .Where(i => string.Equals(serverId, i.ServerId, System.StringComparison.OrdinalIgnoreCase))
                    .Select(i => i.ItemId)
                    .ToList();

                return Task.FromResult(list);
            }
            catch (IOException)
            {
                var list = new List<string>();

                return Task.FromResult(list);
            }
        }

        public Task Delete(string id)
        {
            File.Delete(GetPath(id));
            return Task.FromResult(true);
        }
    }
}
