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
        public static int userId = UserId();

        private static int UserId()
        {
            int rtnVal;

            try
            {
                // When logged into the Umbraco back office
                if (!String.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
                {
                    rtnVal = Convert.ToInt32(ApplicationContext.Current.Services.UserService.GetByUsername(HttpContext.Current.User.Identity.Name).Id);
                }
                else
                {
                    // When called from an API
                    var configured = new MediaSyncConfigurationFromXml().ReadIntegerSetting("userId");
                    return configured ?? 0;
                }
            }
            catch (Exception)
            {
                // Default to 0 (admin)
                rtnVal = 0;
            }

            return rtnVal;
        }
    }
}