using AX.DataRepository.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace AX.DataRepository
{
    public interface IDataRepository
    {
        int? CommandTimeout { get; set; }
        DbConnection DBConnection { get; }
        DataBaseType DBType { get; }
        bool UseBuffered { get; set; }

        void AbortTransaction();
        Task AbortTransactionAsync();
        List<T> BatchInsert<T>(List<T> entities);
        Task<List<T>> BatchInsertAsync<T>(List<T> entities);
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void CompleteTransaction();
        Task CompleteTransactionAsync();
        int Delete<T>(string whereSql, dynamic param);
        int Delete<T>(T entity);
        int DeleteAll<T>();
        Task<int> DeleteAllAsync<T>();
        Task<int> DeleteAsync<T>(string whereSql, dynamic param);
        Task<int> DeleteAsync<T>(T entity);
        int DeleteById<T>(dynamic id);
        Task<int> DeleteByIdAsync<T>(dynamic id);
        void Dispose();
        int ExecuteNonQuery(string sql, dynamic param);
        Task<int> ExecuteNonQueryAsync(string sql, dynamic param);
        T ExecuteScalar<T>(string sql, dynamic param);
        Task<T> ExecuteScalarAsync<T>(string sql, dynamic param);
        List<T> GetAll<T>();
        Task<List<T>> GetAllAsync<T>();
        int GetCount<T>();
        int GetCount<T>(FetchParameter fetchParameter);
        int GetCount<T>(string whereSql, dynamic param);
        Task<int> GetCountAsync<T>();
        Task<int> GetCountAsync<T>(FetchParameter fetchParameter);
        Task<int> GetCountAsync<T>(string whereSql, dynamic param);
        string GetCreateTableSql(Type type);
        string GetCreateTableSql<T>();
        DataTable GetDataTable(string sql, dynamic param);
        Task<DataTable> GetDataTableAsync(string sql, dynamic param);
        PageResult<T> GetList<T>(FetchParameter fetchParameter);
        List<T> GetList<T>(string whereSql, dynamic param);
        Task<PageResult<T>> GetListAsync<T>(FetchParameter fetchParameter);
        Task<List<T>> GetListAsync<T>(string whereSql, dynamic param);
        T Insert<T>(T entity);
        Task<T> InsertAsync<T>(T entity);
        T SingleOrDefault<T>(string whereSql, dynamic param);
        Task<T> SingleOrDefaultAsync<T>(string whereSql, dynamic param);
        T SingleOrDefaultById<T>(dynamic PrimaryKey);
        Task<T> SingleOrDefaultByIdAsync<T>(dynamic PrimaryKey);
        bool TestConnection();
        int Update<T>(T entity);
        int Update<T>(T entity, string fields);
        Task<int> UpdateAsync<T>(T entity);
        Task<int> UpdateAsync<T>(T entity, string fields);
        string UpdateSchema(Type type, bool execute);
        string UpdateSchema<T>(bool execute);
    }
}