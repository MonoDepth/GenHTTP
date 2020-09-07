using System;
using System.Collections.Generic;
using System.Reflection;

using GenHTTP.Api.Content;

using GenHTTP.Modules.Core.Conversion;

namespace GenHTTP.Modules.Webservices
{

    public class ResourceRouter : AbstractMethodRouter<ResourceMethodProvider, ResourceMethodAttribute>
    {

        #region Get-/Setters

        #endregion

        #region Initialization

        public ResourceRouter(IHandler parent, object instance, SerializationRegistry formats)
            : base(parent, instance, formats)
        {

        }

        protected override IEnumerable<ResourceMethodProvider> AnalyzeMethods(Type type, object instance, SerializationRegistry formats)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = method.GetCustomAttribute<ResourceMethodAttribute>(true);

                if (attribute != null)
                {
                    yield return new ResourceMethodProvider(Parent, method, instance, attribute, formats);
                }
            }
        }

        #endregion

    }

}
