using AX.DataRepository.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace AX.DataRepository
{
    public interface IDataRepository
    {
        int? CommandTimeout { get; set; }
        IDbConnection DBConnection { get; }
        DataBaseType DBType { get; }
        bool UseBuffered { get; set; }

        void AbortTransaction();
        List<T> BatchInsert<T>(List<T> entities);
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void CompleteTransaction();
        int Delete<T>(string whereSql, dynamic param);
        int Delete<T>(T entity);
        int DeleteAll<T>();
        int DeleteById<T>(dynamic id);
        void Dispose();
        int ExecuteNonQuery(string sql, dynamic param);
        T ExecuteScalar<T>(string sql, dynamic param);
        List<T> GetAll<T>();
        int GetCount<T>();
        int GetCount<T>(FetchParameter fetchParameter);
        int GetCount<T>(string whereSql, dynamic param);
        string GetCreateTableSql(Type type);
        string GetCreateTableSql<T>();
        DataTable GetDataTable(string sql, dynamic param);
        PageResult<T> GetList<T>(FetchParameter fetchParameter);
        List<T> GetList<T>(string whereSql, dynamic param);
        T Insert<T>(T entity);
        T SingleOrDefault<T>(string whereSql, dynamic param);
        T SingleOrDefaultById<T>(dynamic PrimaryKey);
        bool TestConnection();
        int Update<T>(T entity);
        int Update<T>(T entity, string fields);
        string UpdateSchema(Type type, bool execute);
        string UpdateSchema<T>(bool execute);
    }
}