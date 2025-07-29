using JobMan.Storage.SqlServer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JobMan.Tests;



public class SqlServerNativeStorageTests : StorageTestBase<DbFixture>
{
    
    string connectionString;
    //TODO: to config

    public SqlServerNativeStorageTests(DbFixture fixture) : base(fixture)
    {
        this.connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=test_jobman;User Id=utest;Password=h354Msd782A;MultipleActiveResultSets=true;Pooling=true;Min Pool Size=10; Max Pool Size=500;TrustServerCertificate=True";
        this.fixture.Storage = new SqlServerNativeStorage(this.connectionString);
    }

    protected override void ClearData(IWorkItemStorage storage)
    {
        SqlServerNativeStorage _storage = (SqlServerNativeStorage)storage;
        _storage.CheckConnectionState();
        DmlCommandCreator dmlCc = (DmlCommandCreator)_storage.DmlCommandCreator;

        using (var command = dmlCc.CreateCommand())
        {
            command.CommandText = "truncate table jm_jobs";
            command.ExecuteNonQuery();
        }

    }


    
}
