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
    public class SqlMapper<T> where T : class
    {
        private List<Action<T, DbDataReader>> mappers = new List<Action<T, DbDataReader>>();

        public SqlMapper(bool autoMap = false)
        {
            if (!autoMap)
                return;

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach(PropertyInfo property in properties)
            {
                string name = property.Name;
                Map(property, name);
            }
        }

        #region setup mappings
        public SqlMapper<T> Map<TValue>(Expression<Func<T, TValue>> expression, string source = "")
        {
            var member = expression.Body as MemberExpression;
            if (string.IsNullOrWhiteSpace(source))
            {
                source = member.Member.Name;
            }

            PropertyInfo prop = (PropertyInfo)member.Member;
            return Map(prop, source);
        }

        public SqlMapper<T> Map(PropertyInfo prop, string source)
        {
            MethodInfo setProp = prop.GetSetMethod();
            Type propType = prop.PropertyType;

            bool isNullable = !propType.IsGenericType || propType.GetGenericTypeDefinition() == typeof(Nullable<>);

            Action<T, DbDataReader> mySetAction = null;

            if (isNullable)//setup nullable check, this will be nullable
            {
                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propType = Nullable.GetUnderlyingType(propType);
                }
                mySetAction = (target, reader) =>
                {
                    var targetVal = reader[source];
                    if (targetVal == DBNull.Value || targetVal == null)
                        setProp.Invoke(target, new object[] { null });
                    else
                        setProp.Invoke(target, new object[] { Convert.ChangeType(targetVal, propType) });
                };
            }
            else
            {
                mySetAction = (target, reader) =>
                {
                    var targetVal = reader[source];
                    if (targetVal == DBNull.Value || targetVal == null)
                    {
                        targetVal = Activator.CreateInstance(propType);
                    }

                    setProp.Invoke(target, new object[] { Convert.ChangeType(targetVal, propType) });
                };
            }

            mappers.Add(mySetAction);
            return this;
        }
        #endregion

        #region synchronous
        public List<T> Map(DbDataReader reader)
        {
            List<T> mappedList = new List<T>();

            if(reader.HasRows)
            {
                while(reader.Read())
                {
                    T mapped = (T)Activator.CreateInstance(typeof(T));
                    foreach (Action<T, DbDataReader> mapper in mappers)
                    {
                        mapper.Invoke(mapped, reader);
                    }
                    mappedList.Add(mapped);
                }
            }

            return mappedList;
        }

        public List<T> Map(SqlDataReader reader) { return Map(reader as DbDataReader); }

        public List<T> Map(DataView view)
        {
            return Map(view.ToTable().CreateDataReader() as DbDataReader);
        }

        public List<T> Map(DataTable table)
        {
            return Map(table.CreateDataReader() as DbDataReader);
            //List<T> mappedList = new List<T>();

            //foreach(DataRow row in table.Rows)
            //{
            //    T mapped = (T)Activator.CreateInstance(typeof(T));

            //    foreach(Action<T, DataRow> mapper in mappers)
            //    {
            //        mapper.Invoke(mapped, row);
            //    }

            //    mappedList.Add(mapped);
            //}

            //return mappedList;
        }
        #endregion

        #region asynchronous
        public async Task<List<T>> MapAsync(DbDataReader reader)
        {
            List<T> mappedList = new List<T>();

            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    T mapped = (T)Activator.CreateInstance(typeof(T));
                    foreach (Action<T, DbDataReader> mapper in mappers)
                    {
                        mapper.Invoke(mapped, reader);
                    }
                    mappedList.Add(mapped);
                }
            }

            return mappedList;
        }

        public async Task<List<T>> MapAsync(SqlDataReader reader) { return await MapAsync(reader as DbDataReader); }

        public async Task<List<T>> MapAsync(DataView view)
        {
            return await MapAsync(view.ToTable().CreateDataReader() as DbDataReader);
        }

        public async Task<List<T>> MapAsync(DataTable table)
        {
            return await MapAsync(table.CreateDataReader() as DbDataReader);
        }
        #endregion
    }
}
