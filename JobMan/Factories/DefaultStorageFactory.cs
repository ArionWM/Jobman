using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan.Factories;

//TODO: create interface and use DI
internal static class DefaultStorageFactory
{
    static Dictionary<string, IWorkItemStorage> storages = new();

    public static StorageOptions DefaultOptions { get; set; }

    public static IWorkItemStorage Get(StorageOptions options)
    {
        string key = options.ConnectionString;
        if (string.IsNullOrWhiteSpace(key))
            key = "Default_" + options.StorageType.FullName;

        IWorkItemStorage _storage = storages.GetOr(key, () =>
        {
            IWorkItemStorage storage = Activator.CreateInstance(options.StorageType, options.ConnectionString) as IWorkItemStorage;
            return storage;
        });

        return _storage;
    }

    public static T Get<T>(string connectionString)
        where T : IWorkItemStorage
    {
        StorageOptions options = new StorageOptions<T>(connectionString);
        return (T)Get(options);
    }
}
