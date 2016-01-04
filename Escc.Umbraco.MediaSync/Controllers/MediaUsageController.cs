using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Escc.Umbraco.MediaSync.DataTypes;
using Exceptionless;
using Umbraco.Web.WebApi;

namespace Escc.Umbraco.MediaSync.Controllers
{
    public class MediaUsageController : UmbracoApiController
    {
        /// <summary>
        /// Checks the authorisation token passed with the request is valid, so that this method cannot be called without knowing the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static bool CheckAuthorisationToken(string token)
        {
            return token == ConfigurationManager.AppSettings["Escc.Umbraco.Inception.AuthToken"];
        }
        
        /// <summary>
        /// Creates the supporting types (eg data types) needed for <see cref="CreateUmbracoDocumentTypes"/> to succeed.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateUmbracoDataTypes([FromUri] string token)
        {
            if (!CheckAuthorisationToken(token)) return Request.CreateResponse(HttpStatusCode.Forbidden);

            try
            {
                MediaContentUsageDataType.CreateDataType();

                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception e)
            {
                e.ToExceptionless().Submit();
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
