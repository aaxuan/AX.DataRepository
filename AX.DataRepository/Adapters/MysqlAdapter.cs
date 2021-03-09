using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace AX.DataRepository.Adapters
{
    public class MySqlAdapter : IAdapter
    {
        public string LeftEscapeChar { get { return "`"; } }

        public string RightEscapeChar { get { return "`"; } }

        public string DbParmChar { get { return "@"; } }

        private string GetType(PropertyInfo item)
        {
            var lowerName = item.PropertyType.FullName.ToLower();

            if (lowerName.Contains("boolean"))
            { return "bit(1)"; }

            if (lowerName.Contains("datetime"))
            { return "datetime"; }

            if (lowerName.Contains("long"))
            { return "bigint"; }
            if (lowerName.Contains("decimal"))
            { return "decimal(10, 2)"; }
            if (lowerName.Contains("double"))
            { return "double"; }
            if (lowerName.Contains("int"))
            { return "int(11)"; }

            if (lowerName.Contains("string"))
            {
                var length = item.GetCustomAttribute<StringLengthAttribute>(false)?.MaximumLength;
                if (length <= 0 || length == null)
                { return "varchar(255)"; }
                else if (length >= 4000)
                { return "text"; }
                else
                { return $"varchar({length})"; }
            }

            if (lowerName.Contains("byte"))
            { return "blob"; }

            throw new System.NotSupportedException($"未匹配字段对应数据库类型 {item.PropertyType.FullName}");
        }

        private object GetCanNull(PropertyInfo propertyInfo, string KeyName)
        {
            if (propertyInfo.Name == KeyName)
                return "NOT NULL";
            else
                return "NULL";
        }

        public string GetColumnExitSql(PropertyInfo propertyInfo, System.Type type, string dataBaseName)
        {
            var tableName = Util.TypeMaper.GetTableName(type);
            return $"SELECT COUNT(*) FROM information_schema.`COLUMNS` WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{propertyInfo.Name}' AND TABLE_SCHEMA = '{dataBaseName}'";
        }

        public string GetTableExitSql(System.Type type, string dataBaseName)
        {
            var tableName = Util.TypeMaper.GetTableName(type);
            return $"SELECT COUNT(*) FROM information_schema.`TABLES` WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = '{dataBaseName}'";
        }

        public string GetCreateColumnSql(PropertyInfo propertyInfo, System.Type type, string dataBaseName)
        {
            var tableName = Util.TypeMaper.GetTableName(type);
            return $"ALTER TABLE {tableName} ADD COLUMN {propertyInfo.Name.ToLower()} {GetType(propertyInfo)} DEFAULT NULL;";
        }

        public string GetCreateTableSql(System.Type type, string dataBaseName)
        {
            var result = new StringBuilder();
            var tableName = Util.TypeMaper.GetTableName(type);
            var key = Util.TypeMaper.GetSingleKey(type);
            var propertyInfos = Util.TypeMaper.GetProperties(type);

            result.AppendLine($"DROP TABLE IF EXISTS `{tableName}`;");
            result.AppendLine($"CREATE TABLE IF NOT EXISTS `{tableName}` (");

            foreach (var propertyInfo in propertyInfos)
            {
                result.AppendLine($"`{propertyInfo.Name}`    {GetType(propertyInfo)}    {GetCanNull(propertyInfo, key.Name)}    COMMENT '{Util.TypeMaper.GetDisplayName(propertyInfo)}',");
            }

            result.Remove(result.Length - 1, 1);
            result.AppendLine($"PRIMARY KEY(`{key.Name}`)");
            result.AppendLine($") ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COMMENT '{Util.TypeMaper.GetDisplayName(type)}';");
            result.AppendLine();
            result.AppendLine();

            return result.ToString();
        }
    }
}