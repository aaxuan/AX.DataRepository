using AX.DataRepository.Adapters;
using AX.DataRepository.Models;
using AX.DataRepository.Util;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AX.DataRepository
{
    public class DataRepository : IDisposable, IDataRepository
    {
        private IDbTransaction DBTransaction { get; set; }

        private IAdapter Adapter;

        private SqlBuilder SqlBuilder;

        public IDbConnection DBConnection { get; private set; }

        public DataBaseType DBType
        {
            get
            {
                var typeName = DBConnection.GetType().Name.ToLower();
                if (typeName.Contains("mysql"))
                { return DataBaseType.MySql; }
                return DataBaseType.None;
            }
        }

        public int? CommandTimeout { get; set; } = null;

        public bool UseBuffered { get; set; } = false;

        /// <summary>
        /// 使用已有链接初始化
        /// </summary>
        /// <param name="dbConnection"></param>
        public DataRepository(DbConnection dbConnection)
        {
            DBConnection = dbConnection;

            if (DBType == DataBaseType.MySql)
            {
                Adapter = new MySqlAdapter();
                SqlBuilder = new SqlBuilder(Adapter.LeftEscapeChar, Adapter.RightEscapeChar, Adapter.DbParmChar);
            }
        }

        #region 事务

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void AbortTransaction()
        {
            if (DBTransaction == null)
            { throw new Exception("Transaction 对象不存在"); }
            DBTransaction.Rollback();
            DBTransaction.Dispose();
            DBTransaction = null;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void CompleteTransaction()
        {
            if (DBTransaction == null)
            { throw new Exception("Transaction 对象不存在"); }
            DBTransaction.Commit();
            DBTransaction.Rollback();
            DBTransaction.Dispose();
            DBTransaction = null;
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (DBTransaction != null)
            { throw new Exception("Transaction 对象已存在"); }
            DBTransaction = DBConnection.BeginTransaction(isolationLevel);
        }

        #endregion 事务

        public int ExecuteNonQuery(string sql, dynamic param)
        {
            return SqlMapper.Execute(DBConnection, sql, param, DBTransaction, CommandTimeout);
        }

        public T ExecuteScalar<T>(string sql, dynamic param)
        {
            return SqlMapper.ExecuteScalar<T>(DBConnection, sql, param, DBTransaction, CommandTimeout);
        }

        #region 查

        public bool TestConnection()
        {
            var result = ExecuteScalar<string>("SELECT 'test' AS test;", null);
            if (string.IsNullOrWhiteSpace(result))
            { return false; }
            return true;
        }

        public int GetCount<T>()
        {
            var sql = SqlBuilder.BuildSelectCount(TypeMaper.GetTableName<T>()).ToSql();
            return ExecuteScalar<int>(sql, null);
        }

        public int GetCount<T>(string whereSql, dynamic param)
        {
            if (whereSql.TrimStart().ToLower().StartsWith("where", StringComparison.InvariantCulture))
            {
                whereSql = SqlBuilder.BuildSelectCount(TypeMaper.GetTableName<T>()).AppendSql(whereSql).ToSql();
            }
            return ExecuteScalar<int>(whereSql, param);
        }

        public int GetCount<T>(FetchParameter fetchParameter)
        {
            var sql = SqlBuilder.BuildSelectCount(TypeMaper.GetTableName<T>());
            DynamicParameters param = new DynamicParameters();

            if (fetchParameter.HasWhereFilters)
            {
                foreach (var item in fetchParameter.WhereFilters)
                {
                    sql.AppendSql($" {item.FilterName} {item.FilterType} {Adapter.DbParmChar}{item.FilterName} ");
                    param.Add($"{Adapter.DbParmChar}{item.FilterName}", item.FilterValue);
                }
            }

            if (string.IsNullOrWhiteSpace(fetchParameter.GroupBy) == false)
            {
                sql.AppendSql($"  GROUP BY {fetchParameter.GroupBy} ");
            }

            if (fetchParameter.HasHavingFilters)
            {
                foreach (var item in fetchParameter.HavingFilters)
                {
                    sql.AppendSql($" {item.FilterName} {item.FilterType} {Adapter.DbParmChar}{item.FilterName} ");
                    param.Add($"{Adapter.DbParmChar}{item.FilterName}", item.FilterValue);
                }
            }

            return ExecuteScalar<int>(sql.ToSql(), param);
        }

        public T SingleOrDefault<T>(string whereSql, dynamic param)
        {
            if (whereSql.TrimStart().ToLower().StartsWith("where", StringComparison.InvariantCulture))
            {
                whereSql = SqlBuilder.BuildSelect(TypeMaper.GetTableName<T>(), TypeMaper.GetProperties(typeof(T))).AppendSql(whereSql).ToSql();
            }
            return SqlMapper.QuerySingleOrDefault<T>(DBConnection, whereSql, param, DBTransaction, CommandTimeout);
        }

        public T SingleOrDefaultById<T>(dynamic PrimaryKey)
        {
            var keyProperties = TypeMaper.GetSingleKey<T>();
            var sql = SqlBuilder.BuildSelect(TypeMaper.GetTableName<T>(), TypeMaper.GetProperties(typeof(T))).Where().AppendColumnNameEqualsValue(keyProperties).ToSql();
            var param = new DynamicParameters();
            param.Add(Adapter.DbParmChar + keyProperties.Name, PrimaryKey);
            return SqlMapper.QuerySingleOrDefault<T>(DBConnection, sql, param, DBTransaction, CommandTimeout);
        }

        public List<T> GetAll<T>()
        {
            var sql = SqlBuilder.BuildSelect(TypeMaper.GetTableName<T>(), TypeMaper.GetProperties(typeof(T))).ToSql();
            return DBConnection.Query<T>(sql, null, DBTransaction, UseBuffered, CommandTimeout).ToList();
        }

        public List<T> GetList<T>(string whereSql, dynamic param)
        {
            if (whereSql.TrimStart().ToLower().StartsWith("where", StringComparison.InvariantCulture))
            {
                whereSql = SqlBuilder.BuildSelect(TypeMaper.GetTableName<T>(), TypeMaper.GetProperties(typeof(T))).AppendSql(whereSql).ToSql();
            }
            return SqlMapper.Query<T>(DBConnection, whereSql, param, DBTransaction, UseBuffered, CommandTimeout).ToList();
        }

        public PageResult<T> GetList<T>(FetchParameter fetchParameter)
        {
            var sql = new StringBuilder();
            DynamicParameters param = new DynamicParameters();
            if (string.IsNullOrWhiteSpace(fetchParameter.Selectfileld))
            {
                sql.Append($"SELECT * FROM {Adapter.LeftEscapeChar}{TypeMaper.GetTableName<T>()}{Adapter.RightEscapeChar} ");
            }
            else
            {
                sql.Append($"SELECT {fetchParameter.Selectfileld} FROM {Adapter.LeftEscapeChar}{TypeMaper.GetTableName<T>()}{Adapter.RightEscapeChar} ");
            }

            if (fetchParameter.HasWhereFilters)
            {
                foreach (var item in fetchParameter.WhereFilters)
                {
                    sql.Append($" {item.FilterName} {item.FilterType} {Adapter.DbParmChar}{item.FilterName} ");
                    param.Add($"{Adapter.DbParmChar}{item.FilterName}", item.FilterValue);
                }
            }

            if (string.IsNullOrWhiteSpace(fetchParameter.GroupBy) == false)
            {
                sql.Append($" GROUP BY {fetchParameter.GroupBy} ");
            }

            if (fetchParameter.HasHavingFilters)
            {
                foreach (var item in fetchParameter.HavingFilters)
                {
                    sql.Append($" {item.FilterName} {item.FilterType} {Adapter.DbParmChar}{item.FilterName} ");
                    param.Add($"{Adapter.DbParmChar}{item.FilterName}", item.FilterValue);
                }
            }

            if (string.IsNullOrWhiteSpace(fetchParameter.OrderBy) == false)
            {
                sql.Append($" ORDER BY {fetchParameter.OrderBy} ");
            }

            if (fetchParameter.UsePage)
            {
                sql.Append($" LIMIT {fetchParameter.PageIndex * fetchParameter.PageItemCount},{(fetchParameter.PageIndex + 1) * fetchParameter.PageItemCount} ");
            }

            var result = new PageResult<T>();
            result.TotalCount = GetCount<T>(fetchParameter);
            result.PageIndex = fetchParameter.PageIndex;
            result.PageItemCount = fetchParameter.PageItemCount;
            result.Data = DBConnection.Query<T>(sql.ToString(), param, DBTransaction, UseBuffered, CommandTimeout).ToList();

            return result;
        }

        public DataTable GetDataTable(string sql, dynamic param)
        {
            var result = new DataTable();
            result.Load(SqlMapper.ExecuteReader(DBConnection, sql, param, DBTransaction, CommandTimeout));
            return result;
        }

        #endregion 查

        #region 增

        public T Insert<T>(T entity)
        {
            var type = typeof(T);
            var tableName = TypeMaper.GetTableName<T>();
            var allProperties = TypeMaper.GetProperties(type);
            var sql = SqlBuilder.BuildInsert(tableName, allProperties).ToSql();
            this.ExecuteNonQuery(sql, entity);
            return entity;
        }

        public List<T> BatchInsert<T>(List<T> entities)
        {
            var type = typeof(T);
            var tableName = TypeMaper.GetTableName<T>();
            var allProperties = TypeMaper.GetProperties(type);
            var sql = SqlBuilder.BuildInsert(tableName, allProperties).ToSql();
            this.ExecuteNonQuery(sql, entities);
            return entities;
        }

        #endregion 增

        #region 删

        /// <summary>
        /// 删除全部数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int DeleteAll<T>()
        {
            var tableName = TypeMaper.GetTableName<T>();
            var sql = SqlBuilder.BuildDelete(tableName).ToSql();
            return ExecuteNonQuery(sql, null);
        }

        public int DeleteById<T>(dynamic id)
        {
            var keyProperties = TypeMaper.GetSingleKey<T>();
            var sql = SqlBuilder.BuildDelete(TypeMaper.GetTableName<T>()).Where().AppendColumnNameEqualsValue(keyProperties).ToSql();
            var param = new DynamicParameters();
            param.Add($"{Adapter.DbParmChar}{keyProperties.Name}", id);
            return ExecuteNonQuery(sql, param);
        }

        public int Delete<T>(T entity)
        {
            if (entity == null) { return 0; }
            var keyProperties = TypeMaper.GetSingleKey<T>();
            var tableName = TypeMaper.GetTableName<T>();
            var sql = SqlBuilder.BuildDelete(tableName).Where().AppendColumnNameEqualsValue(keyProperties).ToSql();
            return ExecuteNonQuery(sql.ToString(), entity);
        }

        public int Delete<T>(string whereSql, dynamic param)
        {
            if (whereSql.TrimStart().ToLower().StartsWith("where", StringComparison.InvariantCulture))
            {
                whereSql = SqlBuilder.BuildDelete(TypeMaper.GetTableName<T>()).AppendSql(whereSql).ToSql();
            }
            return ExecuteNonQuery(whereSql, param);
        }

        #endregion 删

        #region 改

        public int Update<T>(T entity)
        {
            var type = typeof(T);
            var keyProperties = TypeMaper.GetSingleKey<T>();
            var tableName = TypeMaper.GetTableName<T>();
            var allProperties = TypeMaper.GetProperties(type);
            var noKeyProperties = allProperties.Except(new List<PropertyInfo>() { keyProperties }).ToList();
            var sql = SqlBuilder.BuildUpdate(tableName, noKeyProperties).Where().AppendColumnNameEqualsValue(keyProperties).ToSql();
            return ExecuteNonQuery(sql.ToString(), entity);
        }

        public int Update<T>(T entity, string fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
            { return 0; }
            var type = typeof(T);
            var keyProperties = TypeMaper.GetSingleKey<T>();
            var tableName = TypeMaper.GetTableName<T>();
            var allProperties = TypeMaper.GetProperties(type);

            var arrayfields = fields.ToLower().Split(',');

            allProperties = allProperties.Where(p => arrayfields.Contains(p.Name.ToLower())).ToList();
            var noKeyProperties = allProperties.Except(new List<PropertyInfo>() { keyProperties }).ToList();

            var sql = SqlBuilder.BuildUpdate(tableName, noKeyProperties).Where().AppendColumnNameEqualsValue(keyProperties).ToSql();
            return ExecuteNonQuery(sql.ToString(), entity);
        }

        #endregion 改

        public string GetCreateTableSql<T>()
        {
            return Adapter.GetCreateTableSql(TypeMaper.GetTableName<T>(), TypeMaper.GetSingleKey<T>().Name, TypeMaper.GetProperties(typeof(T)));
        }

        public string UpdateSchema<T>(bool execute)
        {
            var result = new StringBuilder();
            var dbName = DBConnection.Database;
            var tableName = TypeMaper.GetTableName<T>();
            var column = TypeMaper.GetProperties(typeof(T));

            //判断表是否存在
            var exitSql = Adapter.GetTableExitSql(tableName, dbName);
            if (ExecuteScalar<int>(exitSql, null) <= 0)
            { result.Append(GetCreateTableSql<T>()); }
            //判断字段是否存在
            else
            {
                for (int i = 0; i < column.Count; i++)
                {
                    var item = column[i];
                    var filedExitSql = Adapter.GetColumnExitSql(item.Name, tableName, dbName);
                    if (ExecuteScalar<int>(filedExitSql, null) <= 0)
                    {
                        result.Append(Adapter.GetCreateColumnSql(tableName, item));
                    }
                }
            }

            if (execute && result.Length > 0)
            { ExecuteNonQuery(result.ToString(), null); }
            return result.ToString();
        }

        public void Dispose()
        {
            if (DBConnection != null)
            {
                DBConnection.Close();
                DBConnection.Dispose();
            }
        }
    }
}