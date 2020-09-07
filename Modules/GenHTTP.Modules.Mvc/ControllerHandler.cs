using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using System.Collections.Generic;

namespace GenHTTP.Modules.Mvc
{

    public class ControllerHandler<T> : IHandler where T : new()
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public ControllerHandler(IHandler parent)
        {
            Parent = parent;
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            throw new System.NotImplementedException();
        }

        #endregion

    }

}
