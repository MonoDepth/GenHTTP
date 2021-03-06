﻿using System.IO.Compression;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Compression
{

    public class BrotliCompression : ICompressionAlgorithm
    {

        public string Name => "br";

        public Priority Priority => Priority.High;

        public IResponseContent Compress(IResponseContent content)
        {
            return new CompressedResponseContent(content, (target) => new BrotliStream(target, CompressionLevel.Fastest, false));
        }

    }

}
