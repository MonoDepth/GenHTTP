using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.Conversion;

namespace GenHTTP.Modules.Webservices
{

    public abstract class AbstractMethodRouter<TProvider, TMetaData> : IHandler 
        where TMetaData : AbstractMethodAttribute where TProvider : AbstractMethodProvider<TMetaData>
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private List<TProvider> Methods { get; }

        #endregion

        #region Initialization

        protected AbstractMethodRouter(IHandler parent, object instance, SerializationRegistry formats)
        {
            Parent = parent;

            Methods = new List<TProvider>(AnalyzeMethods(instance.GetType(), instance, formats));
        }

        protected abstract IEnumerable<TProvider> AnalyzeMethods(Type type, object instance, SerializationRegistry formats);

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            var methods = FindProviders(request.Target.GetRemaining().ToString());

            if (methods.Any())
            {
                var matchingMethods = methods.Where(m => request.Method.Equals(m.MetaData.RequestMethod)).ToList();

                if (matchingMethods.Count == 1)
                {
                    return matchingMethods.First().Handle(request);
                }
                else if (methods.Count > 1)
                {
                    throw new ProviderException(ResponseStatus.BadRequest, $"There are multiple methods matching '{request.Target.Path}'");
                }
                else
                {
                    throw new ProviderException(ResponseStatus.MethodNotAllowed, $"There is no method of a matching request type");
                }
            }

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            foreach (var method in Methods.Where(m => m.MetaData.RequestMethod == RequestMethod.GET))
            {
                var parts = new List<string>(this.GetRoot(request.Server.Handler, false).Parts);

                WebPath path;

                if (method.ParsedPath == null)
                {
                    path = new WebPath(parts, true);
                }
                else
                {
                    parts.Add(method.ParsedPath.ToString());
                    path = new WebPath(parts, false);
                }

                yield return new ContentElement(path, Path.GetFileName(path.ToString()), path.ToString().GuessContentType() ?? ContentType.ApplicationForceDownload, null);
            }
        }

        private List<TProvider> FindProviders(string path) => Methods.Where(m => m.ParsedPath?.IsMatch(path) ?? false).ToList();

        #endregion

    }

}
