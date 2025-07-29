using JobMan.Storage.SqlServer;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json.Serialization;

namespace JobMan;

public static class SqlServerExtensions
{


    static SqlServerExtensions()
    {

    }

    public static IWorkPoolOptions UseSqlServerStorage(this IWorkPoolOptions options, string connectionString)
    {
        SqlServerNativeStorage sqlServerStorage = new SqlServerNativeStorage(connectionString);
        options.Storage = sqlServerStorage;
        return options;
    }

    public static IWorkServerOptions UseSqlServerStorage(this IWorkServerOptions options, string connectionString)
    {
        SqlServerNativeStorage sqlServerStorage = new SqlServerNativeStorage(connectionString);
        foreach (IWorkPoolOptions poolOptions in options.PoolOptions)
            if (poolOptions.Storage == null)
                poolOptions.Storage = sqlServerStorage;

        return options;
    }



}
