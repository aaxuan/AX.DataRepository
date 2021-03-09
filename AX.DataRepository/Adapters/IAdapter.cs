using System;
using System.Reflection;

namespace AX.DataRepository.Adapters
{
    internal interface IAdapter
    {
        string DbParmChar { get; }
        string LeftEscapeChar { get; }
        string RightEscapeChar { get; }

        string GetColumnExitSql(PropertyInfo propertyInfo, Type type, string dataBaseName);

        string GetTableExitSql(Type type, string dataBaseName);

        string GetCreateColumnSql(PropertyInfo propertyInfo, Type type, string dataBaseName);

        string GetCreateTableSql(Type type, string dataBaseName);
    }
}