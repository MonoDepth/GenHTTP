namespace GenHTTP.Modules.Mvc
{

    public static class Mvc
    {

        public static ControllerBuilder<T> Controller<T>() where T : new() => new ControllerBuilder<T>();

    }

}
