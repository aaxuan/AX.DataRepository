using AX.DataRepository.Models;
using AX.DataRepository.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AX.DataRepository.Tests
{
    [TestClass()]
    public class DataRepositoryTests
    {
        public static IDataRepository DB;

        [ClassInitialize]
        [TestMethod()]
        public static void ClassInitialize(TestContext context)
        {
            DB = new DataRepository(new MySqlConnection("Server=localhost;Database=test;Uid=root;Pwd=root;"));
            var sql1 = DB.UpdateSchema<ObjectX>(true);
            var sql2 = DB.UpdateSchema<ObjectY>(true);
        }

        [TestMethod()]
        public void TransactionTest()
        {
            DB.DeleteAll<ObjectY>();
            DB.Insert(new ObjectY() { ObjectYId = new Random().Next(5, 100), Name = "boby" });
            DB.Insert(new ObjectY() { ObjectYId = new Random().Next(100, 900), Name = "lover" });
            DB.BeginTransaction();
            DB.DeleteAll<ObjectY>();
            DB.AbortTransaction();
            Assert.AreEqual(DB.GetCount<ObjectY>(), 2);
            DB.BeginTransaction();
            DB.DeleteAll<ObjectY>();
            DB.CompleteTransaction();
            Assert.AreEqual(DB.GetCount<ObjectY>(), 0);
        }

        [TestMethod()]
        public void ExecuteNonQueryTest()
        {
            DB.DeleteAll<ObjectY>();
            DB.Insert(new ObjectY() { ObjectYId = new Random().Next(5, 100), Name = "boby" });
            DB.Insert(new ObjectY() { ObjectYId = new Random().Next(100, 900), Name = "lover" });
            Assert.AreEqual(DB.ExecuteNonQuery("update set name = 'xxx' from objecty", null), 2);
        }

        [TestMethod()]
        public void ExecuteScalarTest()
        {
            DB.DeleteAll<ObjectY>();
            var id1 = new ObjectY() { ObjectYId = new Random().Next(100, 900), Name = "lover" };
            DB.Insert(id1);
            Assert.AreEqual(DB.ExecuteScalar<string>("select name from ObjectY where ObjectYId = @id", new { id = id1.ObjectYId }), "lover");
        }

        [TestMethod()]
        public void TestConnectionTest()
        {
            Assert.IsTrue(DB.TestConnection());
        }

        [TestMethod()]
        public void GetCountTest()
        {
            DB.DeleteAll<ObjectX>();
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), NulldateTime = DateTime.Now, NullMoney = 45.23M });
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), NulldateTime = DateTime.Now, NullMoney = 45M });
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), NulldateTime = DateTime.Now, NullMoney = 44.2M });
            Assert.IsTrue(DB.GetCount<ObjectX>() == 3);
        }

        [TestMethod()]
        public void GetCountTest1()
        {
            DB.DeleteAll<ObjectX>();
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45M });
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), NulldateTime = DateTime.Now, NullMoney = 44.2M });
            Assert.IsTrue(DB.GetCount<ObjectX>("where name = @name", new { name = "lover" }) == 2);
        }

        [TestMethod()]
        public void GetCountTest2()
        {
            DB.DeleteAll<ObjectX>();
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45M });
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), NulldateTime = DateTime.Now, NullMoney = 44.2M });
            FetchParameter parameter = new FetchParameter();
            parameter.WhereFilters.Add(new FetchFilter() { FilterName = "name", FilterType = "=", FilterValue = "lover" });
            Assert.IsTrue(DB.GetCount<ObjectX>(parameter) == 2);
        }

        [TestMethod()]
        public void SingleOrDefaultTest()
        {
            DB.DeleteAll<ObjectX>();
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
            Assert.IsNotNull(DB.SingleOrDefault<ObjectX>("where name = @name", new { name = "lover" }));
        }

        [TestMethod()]
        public void SingleOrDefaultByIdTest()
        {
            DB.DeleteAll<ObjectX>();
            var id1 = new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M };
            DB.Insert(id1);
            Assert.IsNotNull(DB.SingleOrDefaultById<ObjectX>(id1.ObjectXId));
        }

        [TestMethod()]
        public void GetAllTest()
        {
            DB.DeleteAll<ObjectX>();
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45M });
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), NulldateTime = DateTime.Now, NullMoney = 44.2M });
            Assert.IsTrue(DB.GetAll<ObjectX>().Count == 3);
        }

        [TestMethod()]
        public void GetListTest()
        {
            DB.DeleteAll<ObjectX>();
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45M });
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), NulldateTime = DateTime.Now, NullMoney = 44.2M });
            Assert.IsTrue(DB.GetList<ObjectX>("where name = 'lover'", null).Count == 2);
        }

        [TestMethod()]
        public void GetListTest1()
        {
            DB.DeleteAll<ObjectX>();

            for (int i = 0; i < 100; i++)
            {
                DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = i.ToString(), Money = i });
            }

            FetchParameter parameter = new FetchParameter();
            parameter.WhereFilters.Add(new FetchFilter() { FilterName = "monry", FilterType = ">", FilterValue = "50" });
            parameter.PageIndex = 0;
            parameter.PageItemCount = 15;

            var result = DB.GetList<ObjectX>(parameter);

            Assert.IsTrue(result.TotalCount == 100);
            Assert.IsTrue(result.Data.Count == 15);
            Assert.IsNotNull(result.Data);
        }

        [TestMethod()]
        public void GetDataTableTest()
        {
            DB.DeleteAll<ObjectX>();
            var list = new List<ObjectX>();
            list.Add(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
            list.Add(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
            list.Add(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
            DB.BatchInsert(list);
            var dt = DB.GetDataTable("select * from objecty", null);
            Assert.IsTrue(dt.Rows.Count == 3);
        }

        [TestMethod()]
        public void InsertTest()
        {
            DB.DeleteAll<ObjectX>();
            DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
        }

        [TestMethod()]
        public void BatchInsertTest()
        {
            DB.DeleteAll<ObjectX>();
            var list = new List<ObjectX>();
            list.Add(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
            list.Add(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
            list.Add(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), Name = "lover", NulldateTime = DateTime.Now, NullMoney = 45.23M });
            DB.BatchInsert(list);
            Assert.IsTrue(DB.GetCount<ObjectY>() == 3);
        }

        [TestMethod()]
        public void DeleteAllTest()
        {
            DB.DeleteAll<ObjectX>();
            var id1 = DB.Insert(new ObjectX { ObjectXId = Guid.NewGuid().ToString(), Name = "Alice", Order = 12 });
            var id2 = DB.Insert(new ObjectX { ObjectXId = Guid.NewGuid().ToString(), Name = "Bob", Order = 56 });
            Assert.IsTrue(DB.DeleteAll<ObjectX>() > 0);
            Assert.IsNull(DB.SingleOrDefaultById<ObjectX>(id1.ObjectXId));
            Assert.IsNull(DB.SingleOrDefaultById<ObjectX>(id2.ObjectXId));
        }

        [TestMethod()]
        public void DeleteByIdTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetCreateTableSqlTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateSchemaTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DisposeTest()
        {
            Assert.Fail();
        }
    }
}