using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.Conversion;

namespace GenHTTP.Modules.Webservices
{

    /// <summary>
    /// Allows to invoke a function on a webservice resource.
    /// </summary>
    /// <remarks>
    /// This provider analyzes the target method to be invoked and supplies
    /// the required arguments. The result of the method is analyzed and
    /// converted into a HTTP response.
    /// </remarks>
    public class ResourceMethodProvider : AbstractMethodProvider<ResourceMethodAttribute>
    {
        private static readonly Regex EMPTY = new Regex("^(/|)$", RegexOptions.Compiled);

        private static readonly Regex VAR_PATTERN = new Regex(@"\:([a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #region Initialization

        public ResourceMethodProvider(IHandler parent, MethodInfo method, object instance, ResourceMethodAttribute metaData, SerializationRegistry serialization)
            : base(parent, method, instance, metaData, serialization)
        {

        }

        protected override Regex DeterminePath(MethodInfo method, object instance, ResourceMethodAttribute metaData)
        {
            var path = metaData.Path;

            if (path != null)
            {
                var builder = new StringBuilder(path);

                // convert parameters of the format ":var" into appropriate groups
                foreach (Match match in VAR_PATTERN.Matches(path))
                {
                    builder.Replace(match.Value, @$"(?<{match.Groups[1].Value}>[a-z0-9]+)");
                }

                return new Regex($"^/{builder.ToString()}$");
            }

            return EMPTY;
        }

        #endregion

        #region Functionality

        protected override IResponse GetResponse(IRequest request, object? result)
        {
            // no result = 204
            if (result == null)
            {
                return request.Respond().Status(ResponseStatus.NoContent).Build();
            }

            var type = result.GetType();

            // response returned by the method
            if (result is IResponseBuilder response)
            {
                return response.Build();
            }

            // stream returned as a download
            if (result is Stream download)
            {
                return request.Respond()
                              .Content(download)
                              .Type(ContentType.ApplicationForceDownload)
                              .Build();
            }

            // basic types should produce a string value
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
            {
                return request.Respond().Content(result.ToString())
                                        .Type(ContentType.TextPlain)
                                        .Build();
            }

            // serialize the result
            var serializer = Serialization.GetSerialization(request);

            if (serializer == null)
            {
                throw new ProviderException(ResponseStatus.UnsupportedMediaType, "Requested format is not supported");
            }

            return serializer.Serialize(request, result)
                             .Build();
        }

        public override IEnumerable<ContentElement> GetContent(IRequest request) => Enumerable.Empty<ContentElement>();

        #endregion

    }

}
