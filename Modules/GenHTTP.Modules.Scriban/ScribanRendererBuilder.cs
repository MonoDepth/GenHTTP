﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;

namespace GenHTTP.Modules.Scriban
{

    public class ScribanRendererBuilder : IBuilder<ScribanRenderer<TemplateModel>>
    {
        protected IResourceProvider? _TemplateProvider;
        
        #region Functionality

        public ScribanRendererBuilder TemplateProvider(IResourceProvider templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public ScribanRenderer<TemplateModel> Build()
        {
            if (_TemplateProvider == null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            return new ScribanRenderer<TemplateModel>(_TemplateProvider);
        }

        #endregion

    }

}
