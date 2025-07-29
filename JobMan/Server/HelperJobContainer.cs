using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class HelperJobContainer
    {
        public static void Clean()
        {
            HashSet<IWorkItemStorage> storages = new HashSet<IWorkItemStorage>();
            foreach (IWorkPool pool in JobManGlobals.Server.Pools)
            {
                storages.Add(pool.Options.Storage);
            }

            foreach (var storage in storages)
            {
                storage.Clean();
            }

        }

    }
}
