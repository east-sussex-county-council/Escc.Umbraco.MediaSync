using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escc.Umbraco.MediaSync.Tests
{
    class TestMediaConfiguration : IMediaSyncConfigurationProvider
    {
        private readonly string[] _propertyEditorAliases;

        public TestMediaConfiguration(string[] propertyEditorAliases)
        {
            this._propertyEditorAliases = propertyEditorAliases;
        }

        public bool ReadBooleanSetting(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ReadPropertyEditorAliases(string mediaIdProvider)
        {
            return _propertyEditorAliases;
        }

        public string ReadSetting(string key)
        {
            throw new NotImplementedException();
        }

        public bool SyncNode(global::Umbraco.Core.Models.IContent content)
        {
            throw new NotImplementedException();
        }
    }
}
