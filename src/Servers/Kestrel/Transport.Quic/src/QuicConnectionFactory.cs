// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Quic;
using System.Net.Security;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Quic.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Server.Kestrel.Transport.Quic;

// Not used anywhere. Remove?
internal sealed class QuicConnectionFactory : IMultiplexedConnectionFactory
{
    private readonly QuicTransportContext _transportContext;

    public QuicConnectionFactory(IOptions<QuicTransportOptions> options, ILoggerFactory loggerFactory)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var logger = loggerFactory.CreateLogger("Microsoft.AspNetCore.Server.Kestrel.Transport.Quic.Client");

        _transportContext = new QuicTransportContext(logger, options.Value);
    }

    public async ValueTask<MultiplexedConnectionContext> ConnectAsync(EndPoint endPoint, IFeatureCollection? features = null, CancellationToken cancellationToken = default)
    {
        if (endPoint is not IPEndPoint)
        {
            throw new NotSupportedException($"{endPoint} is not supported");
        }

        var sslOptions = features?.Get<SslClientAuthenticationOptions>();
        var connection = await QuicConnection.ConnectAsync(new QuicClientConnectionOptions()
        {
            RemoteEndPoint = endPoint,
            ClientAuthenticationOptions = sslOptions ?? new SslClientAuthenticationOptions()
        }, cancellationToken);

        await connection.ConnectAsync(cancellationToken);
        return new QuicConnectionContext(connection, _transportContext);
    }
}
