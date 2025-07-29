using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan;

/// <summary>
/// Using for non service / DI access
/// </summary>
public static class JobManGlobals
{
    private static IWorkServer _server;

    public static ITimeResolver Time { get; set; }
    public static IWorkServer Server
    {
        get { return _server; }
        set
        {
            _server = value;
            WorkServerOptions = _server.Options;
        }
    }

    public static IWorkServerOptions WorkServerOptions { get; set; }

    public static ILoggerFactory LoggerFactory { get; set; }

    static JobManGlobals()
    {
    }
}
