using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

using JobMan.Factories;

namespace JobMan;

public static class WorkServerExtensions
{
    public static IWorkPoolOptions AddPool(this IWorkServerOptions wsopt, string name, Action<IWorkPoolOptions> options = null)
    {
        WorkPoolOptions workPoolOptions = new WorkPoolOptions();
        workPoolOptions.ExecutionPolicy = wsopt.DefaultPolicy;
        workPoolOptions.JobExecutionFilter = wsopt.JobExecutionFilter;

        workPoolOptions.Name = name;
        //workPoolOptions.Logger = wsopt.Logger;
        options?.Invoke(workPoolOptions);
        wsopt.PoolOptions.Add(workPoolOptions);
        return workPoolOptions;
    }

    public static IWorkPoolOptions UseStorage(this IWorkPoolOptions poolOptions, StorageOptions storageOptions)
    {
        if (poolOptions == null)
            throw new ArgumentNullException(nameof(poolOptions));

        if (poolOptions.StorageOptions == null && poolOptions.Storage == null)
            poolOptions.StorageOptions = storageOptions;

        return poolOptions;
    }

    public static IWorkPoolOptions UseStorage<T>(this IWorkPoolOptions poolOptions, string connectionString)
        where T : IWorkItemStorage
    {
        if (poolOptions == null)
            throw new ArgumentNullException(nameof(poolOptions));

        StorageOptions storageOptions = new StorageOptions<T>(connectionString);
        if (poolOptions.StorageOptions == null && poolOptions.Storage == null)
            poolOptions.StorageOptions = storageOptions;


        return poolOptions;
    }

    /// <summary>
    /// Set storage options for all available pools in the server.
    /// "available": if the pool does not have a storage set already.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="storageOptions"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IWorkServerOptions UseStorage(this IWorkServerOptions options, StorageOptions storageOptions)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        DefaultStorageFactory.DefaultOptions = storageOptions;

        foreach (IWorkPoolOptions poolOptions in options.PoolOptions)
            if (poolOptions.StorageOptions == null && poolOptions.Storage == null)
                poolOptions.StorageOptions = storageOptions;

        return options;
    }

    /// <summary>
    /// Set storage for all available pools in the server.
    /// "available": if the pool does not have a storage set already.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IWorkServerOptions UseStorage<T>(this IWorkServerOptions options, string connectionString)
        where T : IWorkItemStorage
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        StorageOptions storageOptions = new StorageOptions<T>(connectionString);

        options.UseStorage(storageOptions);
        return options;
    }

    public static void CheckStorage(this IWorkPoolOptions options)
    {
        if (options.Storage == null)
        {
            if (options.StorageOptions == null)
                throw new ArgumentNullException(nameof(options.Storage), "Storage is not set. Use UseStorage method to set storage for the pool.");

            options.Storage = DefaultStorageFactory.Get(options.StorageOptions);
        }
    }

    public static void CheckStorage(this IWorkPool workpool)
    {
        if (workpool == null)
            throw new ArgumentNullException(nameof(workpool));

        if (workpool.Options == null)
            throw new ArgumentNullException(nameof(workpool.Options));

        workpool.Options.CheckStorage();
    }

    /// <summary>
    /// Add recurring job to the default pool with cron expression
    /// </summary>
    /// <param name="server"></param>
    /// <param name="action"></param>
    /// <param name="cronExpression"></param>
    public static void Schedule(this IWorkServer server, Expression<Action> action, string cronExpression)
    {
        server.Schedule(null, action, cronExpression);
    }

    /// <summary>
    /// Add job to the default pool for run after a certain time
    /// </summary>
    /// <param name="server"></param>
    /// <param name="action"></param>
    /// <param name="runAfter"></param>
    public static void Enqueue(this IWorkServer server, Expression<Action> action, TimeSpan runAfter)
    {
        server.Enqueue(null, action, runAfter);
    }

    /// <summary>
    /// Add job to the default pool
    /// </summary>
    /// <param name="server"></param>
    /// <param name="action"></param>
    public static void Enqueue(this IWorkServer server, Expression<Action> action)
    {
        server.Enqueue(null, action);
    }
}
