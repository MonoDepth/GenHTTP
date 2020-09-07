using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.Conversion;

namespace GenHTTP.Modules.Webservices
{

    /// <summary>
    /// Allows to invoke a function on a service oriented resource.
    /// </summary>
    /// <remarks>
    /// This provider analyzes the target method to be invoked and supplies
    /// the required arguments. The result of the method is analyzed and
    /// converted into a HTTP response.
    /// </remarks>
    public abstract class AbstractMethodProvider<T> : IHandler where T : AbstractMethodAttribute
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        /// <summary>
        /// The path of the method, converted into a regular
        /// expression to be evaluated at runtime.
        /// </summary>
        public Regex ParsedPath { get; }

        public MethodInfo Method { get; }

        public T MetaData { get; }

        protected object Instance { get; }

        protected SerializationRegistry Serialization { get; }

        #endregion

        #region Initialization

        protected AbstractMethodProvider(IHandler parent, MethodInfo method, object instance, T metaData, SerializationRegistry serialization)
        {
            Parent = parent;

            Method = method;
            Instance = instance;
            MetaData = metaData;
            Serialization = serialization;

            ParsedPath = DeterminePath(method, instance, metaData);
        }

        protected abstract Regex DeterminePath(MethodInfo method, object instance, T metaData);

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            return GetResponse(request, Invoke(request));
        }

        private object? Invoke(IRequest request)
        {
            var targetParameters = Method.GetParameters();

            var targetArguments = new object?[targetParameters.Length];

            var sourceParameters = ParsedPath.Match(request.Target.GetRemaining().ToString());

            for (int i = 0; i < targetParameters.Length; i++)
            {
                var par = targetParameters[i];

                // request
                if (par.ParameterType == typeof(IRequest))
                {
                    targetArguments[i] = request;
                    continue;
                }

                // handler
                if (par.ParameterType == typeof(IHandler))
                {
                    targetArguments[i] = this;
                    continue;
                }

                // input stream
                if (par.ParameterType == typeof(Stream))
                {
                    if (request.Content == null)
                    {
                        throw new ProviderException(ResponseStatus.BadRequest, "Request body expected");
                    }

                    targetArguments[i] = request.Content;
                    continue;
                }

                if (par.ParameterType.IsPrimitive || par.ParameterType == typeof(string) || par.ParameterType.IsEnum)
                {
                    // is there a named parameter?
                    var sourceArgument = sourceParameters.Groups[par.Name];

                    if (sourceArgument.Success)
                    {
                        targetArguments[i] = ChangeType(sourceArgument.Value, par.ParameterType);
                        continue;
                    }

                    // is there a query parameter?
                    if (request.Query.TryGetValue(par.Name, out var value))
                    {
                        targetArguments[i] = ChangeType(value, par.ParameterType);
                        continue;
                    }

                    // assume the default value
                    continue;
                }
                else
                {
                    // deserialize from body
                    var deserializer = Serialization.GetDeserialization(request);

                    if (deserializer == null)
                    {
                        throw new ProviderException(ResponseStatus.UnsupportedMediaType, "Requested format is not supported");
                    }

                    if (request.Content == null)
                    {
                        throw new ProviderException(ResponseStatus.BadRequest, "Request body expected");
                    }

                    targetArguments[i] = Task.Run(async () => await deserializer.Deserialize(request.Content, par.ParameterType)).Result;
                    continue;
                }
            }

            try
            {
                return Method.Invoke(Instance, targetArguments);
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                return null; // nop
            }
        }

        protected abstract IResponse GetResponse(IRequest request, object? result);

        public abstract IEnumerable<ContentElement> GetContent(IRequest request);

        private object ChangeType(string value, Type type)
        {
            try
            {
                if (type.IsEnum)
                {
                    return Enum.Parse(type, value);
                }

                return Convert.ChangeType(value, type);
            }
            catch (Exception e)
            {
                throw new ProviderException(ResponseStatus.BadRequest, $"Unable to convert value '{value}' to type '{type}'", e);
            }
        }

        #endregion

    }

}
