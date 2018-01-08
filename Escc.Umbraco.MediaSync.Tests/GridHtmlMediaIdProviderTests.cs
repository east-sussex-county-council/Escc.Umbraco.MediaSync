using NUnit.Framework;
using System;
using System.Linq;

namespace Escc.Umbraco.MediaSync.Tests
{
    [TestFixture]
    public class GridHtmlMediaIdProviderTests
    {
        [Test]
        public void MediaUdiIsFound()
        {
            var provider = new GridHtmlMediaIdProvider(new TestMediaConfiguration(new string[] { "Umbraco.Grid" }));

            var mediaGuids = provider.ReadMediaGuidsFromGridJson(ExampleValues.GridJsonWithHtmlLink);

            Assert.AreEqual(mediaGuids.FirstOrDefault(), new Guid("cee5459177ba48fd8db8739d2a1cc8d0"));
        }
    }
}
