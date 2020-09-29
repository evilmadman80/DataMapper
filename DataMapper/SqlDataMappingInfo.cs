using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataMapper
{
    internal class SqlDataMappingInfo<T> : IDisposable
    {
        private List<SqlMappingInfo<T>> _mappings { get; set; }

        public IEnumerable<SqlMappingInfo<T>> Values
        {
            get
            {
                return _mappings;
            }
        }

        public void Add(PropertyInfo prop)
        {
            SqlMappingInfo<T> map = new SqlMappingInfo<T>(prop, true);

            if (map.SourceColumn != MappingHelpers.DO_NOT_MAP)
            {
                _mappings.Add(map);
            }
        }

        public void Add(PropertyInfo prop, DataColumnCollection sourceColumns)
        {
            SqlMappingInfo<T> map = new SqlMappingInfo<T>(prop, sourceColumns);

            if (map.SourceColumn != MappingHelpers.DO_NOT_MAP && sourceColumns.Contains(map.SourceColumn.ToUpper()))
                _mappings.Add(map);
        }

        public void Add(PropertyInfo prop, string[] sourceColumns)
        {
            SqlMappingInfo<T> map = new SqlMappingInfo<T>(prop, sourceColumns);

            if (map.SourceColumn != MappingHelpers.DO_NOT_MAP && map.SetValue != null)
                _mappings.Add(map);
        }

        public SqlDataMappingInfo() { _mappings = new List<SqlMappingInfo<T>>(); }

        public void Dispose()
        {
            _mappings.Clear();
        }
    }
}
