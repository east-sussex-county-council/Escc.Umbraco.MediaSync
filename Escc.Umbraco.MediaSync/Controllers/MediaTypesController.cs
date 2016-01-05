using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Escc.Umbraco.MediaSync.MediaTypes;
using Exceptionless;
using Umbraco.Inception.CodeFirst;
using Umbraco.Web.WebApi;

namespace Escc.Umbraco.MediaSync.Controllers
{
    public class MediaTypesController : UmbracoApiController
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
        /// Update the built-in Umbraco Media Types
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UpdateUmbracoMediaTypes([FromUri] string token)
        {
            if (!CheckAuthorisationToken(token)) return Request.CreateResponse(HttpStatusCode.Forbidden);

            try
            {

                // Update media types
                UmbracoCodeFirstInitializer.CreateOrUpdateEntity(typeof(FileMediaType));

                UmbracoCodeFirstInitializer.CreateOrUpdateEntity(typeof(ImageMediaType));

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
