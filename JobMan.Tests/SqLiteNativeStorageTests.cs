using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobMan.Tests
{
    public class SqLiteNativeStorageTests : StorageTestBase<DbFixture>
    {
        string connectionString;
        public SqLiteNativeStorageTests(DbFixture fixture) : base(fixture)
        {
            throw new NotImplementedException();
            //this.connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=test_jobman;User Id=utest;Password=h354Msd782A;MultipleActiveResultSets=true;Pooling=true;Min Pool Size=10; Max Pool Size=500;TrustServerCertificate=True";
            //this.fixture.Storage = new SqlServerNativeStorage(this.connectionString);
        }

        protected override void ClearData(IWorkItemStorage storage)
        {
            throw new NotImplementedException();
        }
    }
}
