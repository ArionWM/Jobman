using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JobMan
{
    public class DefaultTypeResolver : ITypeResolver
    {
        object _assemblyPopulateLocker = new object();
        HashSet<Assembly> _assemblies = new HashSet<Assembly>();
        ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        protected void PopulateAssemblies()
        {
            Assembly[] assembliesArray = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly1 in assembliesArray)
                _assemblies.Add(assembly1);
        }

        protected Type SearchAllAssemblies(string name)
        {
            lock (_assemblyPopulateLocker)
            {
                if (_assemblies.Count == 0)
                    this.PopulateAssemblies();
            }

            foreach (Assembly assembly in _assemblies)
            {
                Type type = assembly.GetType(name);
                if (type != null)
                    return type;

            }

            return null;

        }

        public Type Get(string name)
        {
            if (_types.ContainsKey(name))
                return _types[name];

            Type type = Type.GetType(name) ?? this.SearchAllAssemblies(name);
            if (type != null)
                _types[name] = type;

            return type;
        }
    }
}
