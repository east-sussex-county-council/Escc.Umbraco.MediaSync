using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Escc.Umbraco.MediaSync
{
    public class uMediaSyncHelper
    {
        public static string configFile = HttpRuntime.AppDomainAppPath + "/config/uMediaSync.config";
        public static IContentService contentService = ApplicationContext.Current.Services.ContentService;
        public static IMediaService mediaService = ApplicationContext.Current.Services.MediaService;
        public static IRelationService relationService = ApplicationContext.Current.Services.RelationService;
        public static int userId = Convert.ToInt32(ApplicationContext.Current.Services.UserService.GetByUsername(HttpContext.Current.User.Identity.Name).Id);
    }
}