using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan.Storage.SqlServer;

internal interface IDmlCommandCreator
{
    SqlCommand CreateCommand();
    SqlCommand CreateInsert(IWorkItemDefinition workItemDefinition);

    SqlCommand CreateUpdateStatus(IWorkItemDefinition workItemDefinition);

    SqlCommand CreateClean(WorkItemStatus status, DateTime maxTimeUtc);

    SqlCommand CreatePeek(int recordCount, string poolName);
    SqlCommand CreateMetrics();
}
