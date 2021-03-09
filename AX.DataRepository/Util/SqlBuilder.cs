using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AX.DataRepository.Util
{
    public class SqlBuilder
    {
        private StringBuilder Sqlsb = new StringBuilder();
        private string LeftEscapeChar = string.Empty;
        private string RightEscapeChar = string.Empty;
        private string ParmChar = string.Empty;

        public SqlBuilder()
        {
        }

        public SqlBuilder(string leftEscapeChar, string rightEscapeChar, string parmChar)
        {
            LeftEscapeChar = leftEscapeChar;
            RightEscapeChar = rightEscapeChar;
            ParmChar = parmChar;
        }

        private List<string> UseEscapeChar(List<PropertyInfo> propertyInfos)
        {
            return propertyInfos.Select(p => string.Format("{0}{1}{2}", LeftEscapeChar, p.Name, RightEscapeChar)).ToList();
        }

        private List<string> UseParmChar(List<PropertyInfo> propertyInfos)
        {
            return propertyInfos.Select(p => string.Format("{0}{1}", ParmChar, p.Name)).ToList();
        }

        #region SQL语句主体模板

        public SqlBuilder BuildSelect(string tableName)
        {
            Sqlsb.Clear();
            Sqlsb.AppendFormat("SELECT * FROM {0} ", tableName);
            return this;
        }

        public SqlBuilder BuildSelect(string tableName, List<PropertyInfo> selectPops)
        {
            Sqlsb.Clear();
            Sqlsb.AppendFormat("SELECT {0} FROM {1} ", string.Join(",", UseEscapeChar(selectPops)), tableName);
            return this;
        }

        public SqlBuilder BuildSelectCount(string tableName)
        {
            Sqlsb.Clear();
            Sqlsb.Append($"SELECT Count(*) FROM {tableName} ");
            return this;
        }

        public SqlBuilder BuildInsert(string tableName, List<PropertyInfo> insertPops)
        {
            Sqlsb.Clear();
            var props = UseEscapeChar(insertPops);
            var parms = UseParmChar(insertPops);
            Sqlsb.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2}) ", tableName, string.Join(",", props), string.Join(",", parms));
            return this;
        }

        public SqlBuilder BuildUpdate(string tableName, List<PropertyInfo> updatePops)
        {
            Sqlsb.Clear();
            var setValues = updatePops.Select(p => string.Format($" {LeftEscapeChar}{p.Name}{RightEscapeChar} = {ParmChar}{p.Name}"));
            Sqlsb.AppendFormat("UPDATE {0} SET {1} ", tableName, string.Join(",", setValues));
            return this;
        }

        public SqlBuilder BuildDelete(string tableName)
        {
            Sqlsb.Clear();
            Sqlsb.AppendFormat("DELETE FROM {0} ", tableName);
            return this;
        }

        #endregion SQL语句主体模板

        public SqlBuilder Where()
        {
            if (Sqlsb.ToString().ToLower().Contains("where"))
            { Sqlsb.Append(" AND "); }
            else
            { Sqlsb.Append(" WHERE "); }
            return this;
        }

        public SqlBuilder Having()
        {
            if (Sqlsb.ToString().ToLower().Contains("having"))
            { Sqlsb.Append(" AND "); }
            else
            { Sqlsb.Append(" HAVING "); }
            return this;
        }

        public SqlBuilder AppendColumnNameEqualsValue(PropertyInfo propertyInfos)
        {
            Sqlsb.Append($" {propertyInfos.Name} = {ParmChar}{propertyInfos.Name}");
            return this;
        }

        public SqlBuilder AppendSql(string sql)
        {
            if (sql.StartsWith(" "))
            { Sqlsb.Append(sql); }
            else
            { Sqlsb.Append(" ").Append(sql); }
            return this;
        }

        public string ToSql()
        {
            return Sqlsb.ToString();
        }
    }
}