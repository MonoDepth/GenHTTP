using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Webservices
{

    public class ResourceMethodAttribute : AbstractMethodAttribute
    {

        #region Get-/Setters

        /// <summary>
        /// The path this method is availabe at.
        /// </summary>
        public string? Path { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Marks the method as a webservice method.
        /// </summary>
        /// <param name="requestMethod">The HTTP verb used to invoke the method</param>
        /// <param name="path">The path the method should be available at</param>
        public ResourceMethodAttribute(RequestMethod requestMethod = Api.Protocol.RequestMethod.GET, string? path = null)
        {
            RequestMethod = new FlexibleRequestMethod(requestMethod);
            Path = path;
        }

        /// <summary>
        /// Configures the method to be invoked via GET at the given path.
        /// </summary>
        /// <param name="path">The path the method should be available at</param>
        public ResourceMethodAttribute(string path) : this(Api.Protocol.RequestMethod.GET, path)
        {

        }

        #endregion

    }

}
