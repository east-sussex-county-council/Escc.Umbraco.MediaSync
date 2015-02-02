using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Escc.Umbraco.MediaSync
{
    public interface IMediaSyncConfigurationProvider
    {
        string ReadSetting(string key);
        bool ReadBooleanSetting(string key);
        bool SyncNode(IContent content);
        IEnumerable<string> ReadPropertyEditorAliases(string mediaIdProvider);
    }
}
