using System;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Webservices
{

    /// <summary>
    /// Attribute indicating that this method can be invoked
    /// via reflection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class AbstractMethodAttribute : Attribute
    {

        #region Get-/Setters

        /// <summary>
        /// The HTTP verb used to invoke this method.
        /// </summary>
        public FlexibleRequestMethod RequestMethod { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Marks the method as a invokable function.
        /// </summary>
        /// <param name="requestMethod">The HTTP verb used to invoke the method</param>
        public AbstractMethodAttribute(RequestMethod requestMethod = Api.Protocol.RequestMethod.GET)
        {
            RequestMethod = new FlexibleRequestMethod(requestMethod);
        }

        #endregion

    }

}
