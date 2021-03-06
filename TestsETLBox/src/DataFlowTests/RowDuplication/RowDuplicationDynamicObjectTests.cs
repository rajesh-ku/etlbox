using ALE.ETLBox;
using ALE.ETLBox.ConnectionManager;
using ALE.ETLBox.ControlFlow;
using ALE.ETLBox.DataFlow;
using ALE.ETLBox.Helper;
using ALE.ETLBox.Logging;
using ALE.ETLBoxTests.Fixtures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ALE.ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class RowDuplicationDynamicObjectTests
    {
        public SqlConnectionManager SqlConnection => Config.SqlConnection.ConnectionManager("DataFlow");
        public RowDuplicationDynamicObjectTests(DataFlowDatabaseFixture dbFixture)
        {
        }

        [Fact]
        public void NoParameter()
        {
            //Arrange
            TwoColumnsTableFixture source2Columns = new TwoColumnsTableFixture("RowDuplicationSource");
            source2Columns.InsertTestData();

            DbSource source = new DbSource(SqlConnection, "RowDuplicationSource");
            RowDuplication duplication = new RowDuplication();
            MemoryDestination dest = new MemoryDestination();

            //Act
            source.LinkTo(duplication);
            duplication.LinkTo(dest);
            source.Execute();
            dest.Wait();

            //Assert
            Assert.Collection(dest.Data,
                row => { dynamic d = row as dynamic; Assert.True(d.Col1 == 1 && d.Col2 == "Test1"); },
                row => { dynamic d = row as dynamic; Assert.True(d.Col1 == 1 && d.Col2 == "Test1"); },
                row => { dynamic d = row as dynamic; Assert.True(d.Col1 == 2 && d.Col2 == "Test2"); },
                row => { dynamic d = row as dynamic; Assert.True(d.Col1 == 2 && d.Col2 == "Test2"); },
                row => { dynamic d = row as dynamic; Assert.True(d.Col1 == 3 && d.Col2 == "Test3"); },
                row => { dynamic d = row as dynamic; Assert.True(d.Col1 == 3 && d.Col2 == "Test3"); }
            );
        }

       
        [Fact]
        public void WithPredicate()
        {
            //Arrange
            TwoColumnsTableFixture source2Columns = new TwoColumnsTableFixture("RowDuplicationSource");
            source2Columns.InsertTestData();

            DbSource source = new DbSource(SqlConnection, "RowDuplicationSource");
            RowDuplication duplication = new RowDuplication(
                row =>
                {
                    dynamic r = row as dynamic;
                    return r.Col1 == 1 || r.Col2 == "Test3";
                });
            MemoryDestination dest = new MemoryDestination();

            //Act
            source.LinkTo(duplication);
            duplication.LinkTo(dest);
            source.Execute();
            dest.Wait();

            //Assert
            Assert.Collection(dest.Data,
                row => { dynamic d = row as dynamic; Assert.True(d.Col1 == 1 && d.Col2 == "Test1"); },
                row => { dynamic d = row as dynamic; Assert.True(d.Col1 == 1 && d.Col2 == "Test1"); },
                row => { dynamic d = row as dynamic; Assert.True(d.Col1 == 2 && d.Col2 == "Test2"); },
                row => { dynamic d = row as dynamic; Assert.True(d.Col1 == 3 && d.Col2 == "Test3"); },
                row => { dynamic d = row as dynamic; Assert.True(d.Col1 == 3 && d.Col2 == "Test3"); }
            );
        }
    }
}
