using System;
using System.Collections.Generic;
using System.Linq;
using Escc.Umbraco.MediaSync.Models;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Constants = Escc.Umbraco.MediaSync.Helpers.Constants;

namespace Escc.Umbraco.MediaSync.Controllers
{
    [PluginController("Escc")]
    public class MediaUsageApiController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Returns the Content referenced for a Media node
        /// </summary>
        /// <param name="id">Media node id</param>
        /// <returns>JSON</returns>
        public MediaContent GetMediaUsage(int id)
        {
            var mc = new MediaContent {MediaNodeId = id, IsUsed = false, Content = new List<ContentNode>()};

            // RelationService
            var rs = ApplicationContext.Current.Services.RelationService;

            // ContentService
            var cs = ApplicationContext.Current.Services.ContentService;

            // MediaService
            var ms = ApplicationContext.Current.Services.MediaService;

            // Media is parent, get relations of our type
            var relations = rs.GetByChild(ms.GetById(id), Constants.FileRelationTypeAlias);

            // Check for relations
            var relationsList = relations as IList<IRelation> ?? relations.ToList();
            if (!relationsList.Any()) return mc;

            mc.IsUsed = true;

            foreach (var relation in relationsList)
            {
                var csNode = cs.GetById(relation.ParentId);
                var cn = new ContentNode
                {
                    Id = csNode.Id,
                    Name = csNode.Name,
                    Path = csNode.Path,
                    Published = csNode.Published,
                    Trashed = csNode.Trashed,
                    State = csNode.Trashed ? "trashed" : csNode.Published == false ? "unpublished" : "",
                    PathName = NodePathAsNameString(csNode.Id, csNode.Path),
                    Comment = relation.Comment
                };

                mc.Content.Add(cn);
            }

            return mc;
        }

        /// <summary>
        /// Return a path to content as string with node names
        /// </summary>
        /// <param name="n">Node Id</param>
        /// <param name="p">Path</param>
        /// <returns></returns>
        private string NodePathAsNameString(int n, string p)
        {
            // ContentService
            var cs = ApplicationContext.Current.Services.ContentService;

            var tokens = p.Split(',');
            var path = new List<string>();

            foreach (var token in tokens)
            {
                int r;
                if (!Int32.TryParse(token, out r)) continue;

                if (r != -1 && r != -20 && r!= n)
                {
                    path.Add(cs.GetById(r).Name);
                }
                else if (r == -20)
                {
                    path.Add("Recycle Bin");
                }
            }

            return string.Join(" > ", path);
        }
    }
}