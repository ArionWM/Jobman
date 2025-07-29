
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json.Serialization;

using JobMan.Storage.PostgreSql;

namespace JobMan;

public static class PostgreSqlExtensions
{


    static PostgreSqlExtensions()
    {

    }

    public static IWorkPoolOptions UsePostgreSqlStorage(this IWorkPoolOptions options, string connectionString)
    {
        PostgreSqlNativeStorage PostgreSqlStorage = new PostgreSqlNativeStorage(connectionString);
        options.Storage = PostgreSqlStorage;
        return options;
    }

    public static IWorkServerOptions UsePostgreSqlStorage(this IWorkServerOptions options, string connectionString)
    {
        PostgreSqlNativeStorage PostgreSqlStorage = new PostgreSqlNativeStorage(connectionString);
        foreach (IWorkPoolOptions poolOptions in options.PoolOptions)
            if (poolOptions.Storage == null)
                poolOptions.Storage = PostgreSqlStorage;

        return options;
    }




}
