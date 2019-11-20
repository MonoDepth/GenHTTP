﻿using System.IO;
using System.Threading.Tasks;
using System.Buffers;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Core.Protocol;
using GenHTTP.Core.Infrastructure.Configuration;

namespace GenHTTP.Core
{

    internal class RequestParser
    {
        public const int READ_BUFFER_SIZE = 8192;

        private static ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

        #region Get-/Setters

        internal RequestContext Context { get; private set; }

        internal NetworkConfiguration Configuration { get; }

        private Stream InputStream { get; }

        #endregion

        #region Initialization

        internal RequestParser(Stream inputStream, NetworkConfiguration configuration)
        {
            Configuration = configuration;
            InputStream = inputStream;

            Context = new RequestContext();
        }

        #endregion

        #region Network handling

        internal async Task<RequestBuilder?> GetRequest()
        {
            var request = await ParseRequest();

            if (request != null)
            {
                Context = await Context.GetNext();
            }

            return request;
        }

        private async ValueTask<Token> GetToken()
        {
            var token = Context.Scanner.NextToken();

            if (token == Token.Unknown)
            {
                if (await Read())
                {
                    token = Context.Scanner.NextToken();
                }
                else
                {
                    return Token.NoData;
                }
            }

            return token;
        }

        private async Task<bool> Read()
        {
            var buffer = POOL.Rent(READ_BUFFER_SIZE);

            try
            {
                var read = await InputStream.ReadWithTimeoutAsync(buffer, 0, buffer.Length);

                if (read > 0)
                {
                    try
                    {
                        await Context.Buffer.Append(buffer, read);
                        return true;
                    }
                    catch (IOException e)
                    {
                        throw new NetworkException("Error while reading client request", e);
                    }
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

        #endregion

        #region Grammar

        private async Task<RequestBuilder?> ParseRequest()
        {
            var request = Context.Request;
            var scanner = Context.Scanner;

            var token = await GetToken();

            if (token == Token.NoData)
            {
                return null;
            }

            if (token != Token.Method)
            {
                throw new ProtocolException("Method expected");
            }

            request.Type(scanner.Value);

            if (await GetToken() != Token.Url)
            {
                throw new ProtocolException("URI expected");
            }

            request.Path(scanner.Value);

            if (await GetToken() != Token.Protocol)
            {
                throw new ProtocolException("Protocol version expected");
            }

            request.Protocol(scanner.Value);

            var next = await ParseHeader();

            if (next != Token.NewLine)
            {
                throw new ProtocolException("New line expected");
            }

            await LoadContent();

            return request;
        }

        private async Task LoadContent()
        {
            var headers = Context.Request.Headers;

            if (headers.TryGetValue("Content-Length", out string value))
            {
                long length;

                if (!long.TryParse(value, out length))
                {
                    throw new ProtocolException("Content length is expected to be a number");
                }

                var contentParser = new RequestContentParser(length, Configuration);

                var body = await contentParser.GetBody(Context.Buffer, InputStream);

                Context.Request.Content(body);
            }
        }

        private async Task<Token> ParseHeader()
        {
            var request = Context.Request;
            var scanner = Context.Scanner;

            Token token;

            while ((token = await GetToken()) == Token.HeaderDefinition)
            {
                var key = scanner.Value;

                if (await GetToken() != Token.HeaderContent)
                {
                    throw new ProtocolException("Header field value expected");
                }

                request.Header(key, scanner.Value);
            }

            return token;
        }

        #endregion

    }


}
