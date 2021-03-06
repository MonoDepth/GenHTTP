﻿using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Security
{

    public class StrictTransportConcern : IConcern
    {
        private const string HEADER = "Strict-Transport-Security";

        #region Get-/Setters

        public IHandler Parent { get; }

        public IHandler Content { get; }

        public StrictTransportPolicy Policy { get; }

        private string HeaderValue { get; }

        #endregion

        #region Initialization

        public StrictTransportConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, StrictTransportPolicy policy)
        {
            Parent = parent;
            Content = contentFactory(this);

            Policy = policy;
            HeaderValue = GetPolicyHeader();
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            var response = Content.Handle(request);

            if (response != null)
            {
                if (request.EndPoint.Secure)
                {
                    if (!response.Headers.ContainsKey(HEADER))
                    {
                        response[HEADER] = HeaderValue;
                    }
                }
            }

            return response;
        }

        private string GetPolicyHeader()
        {
            var seconds = (int)Policy.MaximumAge.TotalSeconds;

            var result = $"max-age={seconds}";

            if (Policy.IncludeSubdomains)
            {
                result += "; includeSubDomains";
            }

            if (Policy.Preload)
            {
                result += "; preload";
            }

            return result;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

        #endregion

    }

}
