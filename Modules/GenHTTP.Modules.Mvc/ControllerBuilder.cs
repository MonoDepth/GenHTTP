using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Mvc
{

    public class ControllerBuilder<T> : IHandlerBuilder where T : new()
    {

        #region Functionality

        public IHandler Build(IHandler parent)
        {
            return new ControllerHandler<T>(parent);
        }

        #endregion

    }

}
