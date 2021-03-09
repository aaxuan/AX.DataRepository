using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace AX.DataRepository.Util
{
    public static class TypeMaper
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeKeyCache = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> TypeTableNameCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypePropertyCache = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

        public static string GetTableName<T>()
        {
            return GetTableName(typeof(T));
        }

        /// <summary>
        /// 获取类型的表名称
        /// 优先 System.ComponentModel.DataAnnotations.Schema.TableAttribute
        /// 若无则返回类型名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            if (TypeTableNameCache.TryGetValue(type.TypeHandle, out string name)) { return name; }

            var tableAttrName = type.GetCustomAttribute<TableAttribute>(false)?.Name;
            if (tableAttrName != null)
            { name = tableAttrName; }
            else
            { name = type.Name; }

            TypeTableNameCache[type.TypeHandle] = name;
            return name;
        }

        public static PropertyInfo GetSingleKey<T>()
        {
            return GetSingleKey(typeof(T));
        }

        /// <summary>
        /// 获取类型单主键
        /// 优先标注 System.ComponentModel.DataAnnotations.KeyAttribute 的字段，数量不符会抛错
        /// 若无特性标注则默认寻找名为 Id 的字段
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo GetSingleKey(Type type)
        {
            var keys = GetKeys(type);
            if (keys.Count != 1)
            { throw new System.Exception($"框架仅支持单主键 [Key] [{type.FullName}>] Key Count: {keys.Count}"); }
            return keys[0];
        }

        public static List<PropertyInfo> GetProperties(Type type)
        {
            if (TypePropertyCache.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> propertyInfo))
            { return propertyInfo.ToList(); }

            //可写数据库属性
            //var properties = type.GetProperties().Where(IsWriteable).ToArray();

            TypePropertyCache[type.TypeHandle] = type.GetProperties();
            return type.GetProperties().ToList();
        }

        public static string GetDisplayName(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<DisplayAttribute>()?.Name;
        }

        public static string GetDisplayName(Type type)
        {
            return type.GetCustomAttribute<DisplayAttribute>()?.Name;
        }

        #region 私有方法

        private static List<PropertyInfo> GetKeys(Type type)
        {
            if (TypeKeyCache.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> propertyInfo))
            { return propertyInfo.ToList(); }

            var allProperties = GetProperties(type);
            var keyProperties = allProperties.Where(p => p.GetCustomAttributes(true).Any(a => a is KeyAttribute)).ToList();
            if (keyProperties.Count <= 0)
            {
                var idProperties = allProperties.Find(p => string.Equals(p.Name, "id", StringComparison.CurrentCultureIgnoreCase));
                if (idProperties != null)
                { keyProperties.Add(idProperties); }
            }
            if (keyProperties.Count == 0)
            { throw new Exception($"<{type.FullName}> 未找到主键"); }
            TypeKeyCache[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        #endregion 私有方法
    }
}