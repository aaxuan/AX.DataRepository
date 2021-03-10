using AX.DataRepository.Models;
using AX.DataRepository.Util;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AX.DataRepository
{
    public partial class DataRepository
    {
        #region 事务

        /// <summary>
        /// 回滚事务
        /// </summary>
        public async Task AbortTransactionAsync()
        {
            if (DBTransaction == null)
            { throw new Exception("Transaction 对象不存在"); }
            await DBTransaction.RollbackAsync();
            await DBTransaction.DisposeAsync();
            DBTransaction = null;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public async Task CompleteTransactionAsync()
        {
            if (DBTransaction == null)
            { throw new Exception("Transaction 对象不存在"); }
            await DBTransaction.CommitAsync();
            await DBTransaction.DisposeAsync();
            DBTransaction = null;
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (DBTransaction != null)
            { throw new Exception("Transaction 对象已存在"); }
            if (DBConnection.State != ConnectionState.Open)
            { await DBConnection.OpenAsync(); }
            DBTransaction = await DBConnection.BeginTransactionAsync(isolationLevel);
        }

        #endregion 事务

        public async Task<int> ExecuteNonQueryAsync(string sql, dynamic param)
        {
            return await SqlMapper.ExecuteAsync(DBConnection, sql, param, DBTransaction, CommandTimeout);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, dynamic param)
        {
            return await SqlMapper.ExecuteScalarAsync<T>(DBConnection, sql, param, DBTransaction, CommandTimeout);
        }

        #region 查

        public async Task<int> GetCountAsync<T>()
        {
            var sql = SqlBuilder.BuildSelectCount(TypeMaper.GetTableName<T>()).ToSql();
            return await ExecuteScalarAsync<int>(sql, null);
        }

        public async Task<int> GetCountAsync<T>(string whereSql, dynamic param)
        {
            if (whereSql.TrimStart().ToLower().StartsWith("where", StringComparison.InvariantCulture))
            {
                whereSql = SqlBuilder.BuildSelectCount(TypeMaper.GetTableName<T>()).AppendSql(whereSql).ToSql();
            }
            return await ExecuteScalarAsync<int>(whereSql, param);
        }

        public async Task<int> GetCountAsync<T>(FetchParameter fetchParameter)
        {
            var sql = SqlBuilder.BuildSelectCount(TypeMaper.GetTableName<T>());
            DynamicParameters param = new DynamicParameters();

            if (fetchParameter.HasWhereFilters)
            {
                sql.Where();
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
                sql.Having();
                foreach (var item in fetchParameter.HavingFilters)
                {
                    sql.AppendSql($" {item.FilterName} {item.FilterType} {Adapter.DbParmChar}{item.FilterName} ");
                    param.Add($"{Adapter.DbParmChar}{item.FilterName}", item.FilterValue);
                }
            }

            return await ExecuteScalarAsync<int>(sql.ToSql(), param);
        }

        public async Task<T> SingleOrDefaultAsync<T>(string whereSql, dynamic param)
        {
            if (whereSql.TrimStart().ToLower().StartsWith("where", StringComparison.InvariantCulture))
            {
                whereSql = SqlBuilder.BuildSelect(TypeMaper.GetTableName<T>(), TypeMaper.GetProperties(typeof(T))).AppendSql(whereSql).ToSql();
            }
            return await SqlMapper.QueryFirstOrDefaultAsync<T>(DBConnection, whereSql, param, DBTransaction, CommandTimeout);
        }

        public async Task<T> SingleOrDefaultByIdAsync<T>(dynamic PrimaryKey)
        {
            var keyProperties = TypeMaper.GetSingleKey<T>();
            var sql = SqlBuilder.BuildSelect(TypeMaper.GetTableName<T>(), TypeMaper.GetProperties(typeof(T))).Where().AppendColumnNameEqualsValue(keyProperties).ToSql();
            var param = new DynamicParameters();
            param.Add(Adapter.DbParmChar + keyProperties.Name, PrimaryKey);
            return await SqlMapper.QuerySingleOrDefaultAsync<T>(DBConnection, sql, param, DBTransaction, CommandTimeout);
        }

        public async Task<List<T>> GetAllAsync<T>()
        {
            var sql = SqlBuilder.BuildSelect(TypeMaper.GetTableName<T>(), TypeMaper.GetProperties(typeof(T))).ToSql();
            return (await DBConnection.QueryAsync<T>(sql, null, DBTransaction, CommandTimeout)).ToList();
        }

        public async Task<List<T>> GetListAsync<T>(string whereSql, dynamic param)
        {
            if (whereSql.TrimStart().ToLower().StartsWith("where", StringComparison.InvariantCulture))
            {
                whereSql = SqlBuilder.BuildSelect(TypeMaper.GetTableName<T>(), TypeMaper.GetProperties(typeof(T))).AppendSql(whereSql).ToSql();
            }

            IEnumerable<T> result = await SqlMapper.QueryAsync<T>(DBConnection, whereSql, param, DBTransaction, CommandTimeout, CommandType.Text);
            return result.ToList<T>();
        }

        public async Task<PageResult<T>> GetListAsync<T>(FetchParameter fetchParameter)
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
                sql.Append(" WHERE ");
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
                sql.Append(" HAVING ");
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
            result.TotalCount = await GetCountAsync<T>(fetchParameter);
            result.PageIndex = fetchParameter.PageIndex;
            result.PageItemCount = fetchParameter.PageItemCount;
            result.Data = (await DBConnection.QueryAsync<T>(sql.ToString(), param, DBTransaction, CommandTimeout)).ToList();

            return result;
        }

        public async Task<DataTable> GetDataTableAsync(string sql, dynamic param)
        {
            var result = new DataTable();
            result.Load(await SqlMapper.ExecuteReaderAsync(DBConnection, sql, param, DBTransaction, CommandTimeout));
            return result;
        }

        #endregion 查

        #region 增

        public async Task<T> InsertAsync<T>(T entity)
        {
            var type = typeof(T);
            var tableName = TypeMaper.GetTableName<T>();
            var allProperties = TypeMaper.GetProperties(type);
            var sql = SqlBuilder.BuildInsert(tableName, allProperties).ToSql();
            await this.ExecuteNonQueryAsync(sql, entity);
            return entity;
        }

        public async Task<List<T>> BatchInsertAsync<T>(List<T> entities)
        {
            var type = typeof(T);
            var tableName = TypeMaper.GetTableName<T>();
            var allProperties = TypeMaper.GetProperties(type);
            var sql = SqlBuilder.BuildInsert(tableName, allProperties).ToSql();
            await this.ExecuteNonQueryAsync(sql, entities);
            return entities;
        }

        #endregion 增

        #region 删

        /// <summary>
        /// 删除全部数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> DeleteAllAsync<T>()
        {
            var tableName = TypeMaper.GetTableName<T>();
            var sql = SqlBuilder.BuildDelete(tableName).ToSql();
            return await ExecuteNonQueryAsync(sql, null);
        }

        public async Task<int> DeleteByIdAsync<T>(dynamic id)
        {
            var keyProperties = TypeMaper.GetSingleKey<T>();
            var sql = SqlBuilder.BuildDelete(TypeMaper.GetTableName<T>()).Where().AppendColumnNameEqualsValue(keyProperties).ToSql();
            var param = new DynamicParameters();
            param.Add($"{Adapter.DbParmChar}{keyProperties.Name}", id);
            return await ExecuteNonQueryAsync(sql, param);
        }

        public async Task<int> DeleteAsync<T>(T entity)
        {
            if (entity == null) { return 0; }
            var keyProperties = TypeMaper.GetSingleKey<T>();
            var tableName = TypeMaper.GetTableName<T>();
            var sql = SqlBuilder.BuildDelete(tableName).Where().AppendColumnNameEqualsValue(keyProperties).ToSql();
            return await ExecuteNonQueryAsync(sql.ToString(), entity);
        }

        public async Task<int> DeleteAsync<T>(string whereSql, dynamic param)
        {
            if (whereSql.TrimStart().ToLower().StartsWith("where", StringComparison.InvariantCulture))
            {
                whereSql = SqlBuilder.BuildDelete(TypeMaper.GetTableName<T>()).AppendSql(whereSql).ToSql();
            }
            return await ExecuteNonQueryAsync(whereSql, param);
        }

        #endregion 删

        #region 改

        public async Task<int> UpdateAsync<T>(T entity)
        {
            var type = typeof(T);
            var keyProperties = TypeMaper.GetSingleKey<T>();
            var tableName = TypeMaper.GetTableName<T>();
            var allProperties = TypeMaper.GetProperties(type);
            var noKeyProperties = allProperties.Except(new List<PropertyInfo>() { keyProperties }).ToList();
            var sql = SqlBuilder.BuildUpdate(tableName, noKeyProperties).Where().AppendColumnNameEqualsValue(keyProperties).ToSql();
            return await ExecuteNonQueryAsync(sql.ToString(), entity);
        }

        public async Task<int> UpdateAsync<T>(T entity, string fields)
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
            return await ExecuteNonQueryAsync(sql.ToString(), entity);
        }

        #endregion 改
    }
}