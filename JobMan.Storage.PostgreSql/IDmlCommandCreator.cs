using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan.Storage.PostgreSql;

internal interface IPostgreDmlCommandCreator
{
    NpgsqlCommand CreateCommand();
    NpgsqlCommand CreateInsert(IWorkItemDefinition workItemDefinition);

    NpgsqlCommand CreateUpdateStatus(IWorkItemDefinition workItemDefinition);

    NpgsqlCommand CreateClean(WorkItemStatus status, DateTime maxTimeUtc);

    NpgsqlCommand CreatePeek(int recordCount, string poolName);
    NpgsqlCommand CreateMetrics();
}
