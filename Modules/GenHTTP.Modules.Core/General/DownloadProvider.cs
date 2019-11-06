﻿using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class DownloadProvider : ContentProviderBase
    {

        #region Get-/Setters

        public IResourceProvider ResourceProvider { get; }

        public ContentType ContentType { get; }

        #endregion

        #region Initialization

        public DownloadProvider(IResourceProvider resourceProvider, ContentType contentType, ResponseModification? mod) : base(mod)
        {
            ResourceProvider = resourceProvider;
            ContentType = contentType;
        }

        #endregion

        #region Functionality

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            if (request.HasType(RequestMethod.GET, RequestMethod.HEAD))
            {
                return request.Respond()
                              .Content(ResourceProvider.GetResource(), ContentType);
            }

            return request.Respond(ResponseStatus.MethodNotAllowed);
        }

        #endregion

    }

}
