using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace DataMapper
{
    public static class SqlMapper
    {
        private static void MapType<T>(T target, object source, PropertyInfo prop)
        {
            try
            {
                //since convert does not work with nullable types we must check and get the underlying type
                Type targetType = prop.PropertyType;
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                if (targetType == typeof(string))
                {
                    prop.SetMethod.Invoke(target, new object[] { source.ToString().Trim() });
                }
                else
                {
                    prop.SetMethod.Invoke(target, new object[] { Convert.ChangeType(source, targetType) });
                }
            }
            catch (Exception) { }
        }

        public static List<T> Map<T>(SqlDataReader source) where T : class
        {
            List<T> mappedList = new List<T>();
            if (source == null || !source.HasRows)
                return mappedList;
            //Dictionary<PropertyInfo, string> propertyMap = new Dictionary<PropertyInfo, string>();
            //Dictionary<PropertyInfo, Action<T, object>> setMap = new Dictionary<PropertyInfo, Action<T, object>>();
            using (SqlDataMappingInfo<T> propMap = new SqlDataMappingInfo<T>())
            {
                try
                {

                    string[] columns = new string[source.FieldCount];
                    for (int i = 0; i < source.FieldCount; ++i)
                        columns[i] = source.GetName(i).ToUpper().Trim();

                    IEnumerable<PropertyInfo> properties = typeof(T).GetProperties();

                    if (properties != null && properties.Count() > 0)
                        properties = properties.Where(pi => pi.CanWrite);


                    if (properties == null || properties.Count() == 0)
                    {
                        return null;
                    }

                    foreach (PropertyInfo prop in properties)
                    {
                        propMap.Add(prop, columns);
                    }
                }
                catch (Exception) { }

                var constructor = new ConstructorDelegate<T>(() => (T)Activator.CreateInstance(typeof(T)));

                do
                {
                    while (source.Read())
                    {
                        T map = constructor(); //(T)Activator.CreateInstance(typeof(T));

                        #region set properties
                        foreach (SqlMappingInfo<T> mapping in propMap.Values) //(PropertyInfo propInfo in properties)
                        {
                            if (mapping?.SetValue == null || source.IsDBNull(mapping.ColumnNumber))
                                continue;

                            mapping.SetValue(map, source.GetValue(mapping.ColumnNumber));
                        }
                        #endregion set properties

                        mappedList.Add(map);
                    }
                } while (source.NextResult());
            }
            return mappedList;
        }

        public static List<T> Map<T>(DataTable source) where T : class
        {
            List<T> mappedList = new List<T>();
            using (SqlDataMappingInfo<T> propMap = new SqlDataMappingInfo<T>())
            {
                try
                {
                    IEnumerable<PropertyInfo> properties = typeof(T).GetProperties();

                    if (properties != null && properties.Count() > 0)
                        properties = properties.Where(pi => pi.CanWrite);

                    if (properties == null || properties.Count() == 0)
                    {
                        return null;
                    }

                    foreach (PropertyInfo prop in properties)
                    {
                        propMap.Add(prop, source.Columns);
                    }
                }
                catch (Exception) { }

                var constructor = new ConstructorDelegate<T>(() => (T)Activator.CreateInstance(typeof(T))); //CreateConstructor<T>();

                foreach (DataRow row in source.Rows)
                {
                    T map = constructor();// (T)Activator.CreateInstance(typeof(T));

                    #region set properties
                    foreach (SqlMappingInfo<T> mapping in propMap.Values) //(PropertyInfo propInfo in properties)
                    {
                        if (mapping?.SetValue == null || row.IsNull(mapping.ColumnNumber))
                            continue;

                        mapping.SetValue(map, row[mapping.ColumnNumber]);
                    }
                    #endregion set properties

                    mappedList.Add(map);
                }
            }
            return mappedList;
        }

        public static List<T> MapSingleSet<T>(SqlDataReader source) where T : class
        {
            List<T> mappedList = new List<T>();
            if (source == null || !source.HasRows)
                return mappedList;
            //Dictionary<PropertyInfo, string> propertyMap = new Dictionary<PropertyInfo, string>();
            //Dictionary<PropertyInfo, Action<T, object>> setMap = new Dictionary<PropertyInfo, Action<T, object>>();
            using (SqlDataMappingInfo<T> propMap = new SqlDataMappingInfo<T>())
            {
                try
                {
                    string[] columns = new string[source.FieldCount];
                    for (int i = 0; i < source.FieldCount; ++i)
                        columns[i] = source.GetName(i).Trim().ToUpper();

                    IEnumerable<PropertyInfo> properties = typeof(T).GetProperties();

                    if (properties == null || properties.Count() == 0)
                        return null;

                    foreach (PropertyInfo prop in properties)
                    {
                        if (!prop.CanWrite) continue;

                        propMap.Add(prop, columns);
                    }
                }
                catch (Exception) { }

                var constructor = new ConstructorDelegate<T>(() => (T)Activator.CreateInstance(typeof(T)));

                while (source.Read())
                {
                    T map = constructor(); //(T)Activator.CreateInstance(typeof(T));

                    #region set properties
                    foreach (SqlMappingInfo<T> mapping in propMap.Values) //(PropertyInfo propInfo in properties)
                    {
                        if (mapping?.SetValue == null || source.IsDBNull(mapping.ColumnNumber))
                            continue;

                        mapping.SetValue(map, source.GetValue(mapping.ColumnNumber));
                    }
                    #endregion set properties

                    mappedList.Add(map);
                }
            }
            return mappedList;
        }

        public static T MapSingle<T>(DataTable source) where T : class
        {
            T returnValue = null;

            if (source == null || source.Rows.Count == 0)
                return returnValue;

            PropertyInfo[] properties = typeof(T).GetProperties();

            DataRow row = source.Rows[0];
            returnValue = (T)Activator.CreateInstance(typeof(T));

            #region set properties
            foreach (PropertyInfo prop in properties.Where(pi => pi.CanWrite))
            {
                string sourceColumn = MappingHelpers.GetSourceColumn(prop);
                if (sourceColumn != MappingHelpers.DO_NOT_MAP)
                {
                    int col = source.Columns.IndexOf(sourceColumn);
                    if (col < 0 || row.IsNull(col))
                        continue;

                    MapType<T>(returnValue, row[col], prop);
                }

            }
            #endregion set properties

            return returnValue;
        }

        public static T MapSingle<T>(SqlDataReader source) where T : class
        {
            T returnValue = null;

            if (source == null || !source.HasRows)
                return returnValue;

            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties();

            if (properties != null && properties.Count() > 0)
                properties = properties.Where(pi => pi.CanWrite);

            if (source.Read())
            {
                returnValue = (T)Activator.CreateInstance(typeof(T));

                #region set properties
                foreach (PropertyInfo prop in properties)
                {
                    try
                    {
                        string sourceColumn = MappingHelpers.GetSourceColumn(prop);
                        int col = source.GetOrdinal(sourceColumn);
                        if (col < 0 || source.IsDBNull(col))
                            continue;

                        MapType<T>(returnValue, source.GetValue(col), prop);
                    }
                    catch (Exception) { }
                }
                #endregion set properties
            }

            return returnValue;
        }
    }
    public class SqlMapper<T> : ISqlMapper<T> where T : class, new()
    {
        public List<T> Map(DbDataReader reader)
        {
            throw new NotImplementedException();
        }

        public List<T> Map(DataTable table)
        {
            throw new NotImplementedException();
        }

        public T MapSingle(DbDataReader reader)
        {
            throw new NotImplementedException();
        }

        public T MapSingle(DataRow row)
        {
            throw new NotImplementedException();
        }

        public SqlMapper()
        {

        }
    }
}
