﻿using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core.General
{

    public abstract class RouterBase : IRouter
    {
        private IRouter? _Parent;

        #region Get-/Setters
        
        public IRouter Parent
        {
            get { return _Parent ?? throw new InvalidOperationException("Parent has not been set"); }
            set { _Parent = value; }
        }

        protected IRenderer<TemplateModel>? Template { get; }

        protected IContentProvider? ErrorHandler { get; }

        #endregion

        #region Initialization

        protected RouterBase(IRenderer<TemplateModel>? template, IContentProvider? errorHandler)
        {
            Template = template;
            ErrorHandler = ErrorHandler;
        }

        #endregion

        #region Functionality
        
        public IRenderer<TemplateModel> GetRenderer()
        {
            return Template ?? Parent.GetRenderer();
        }

        public IContentProvider GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause)
        {
            return ErrorHandler ?? Parent.GetErrorHandler(request, responseType, cause);
        }

        public abstract void HandleContext(IEditableRoutingContext current);

        public abstract string? Route(string path, int currentDepth);

        #endregion

    }

}