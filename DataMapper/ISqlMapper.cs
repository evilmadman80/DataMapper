using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataMapper
{
    public interface ISqlMapper<T> where T : class, new()
    {
        T MapSingle(System.Data.Common.DbDataReader reader);

        T MapSingle(System.Data.DataRow row);

        List<T> Map(System.Data.Common.DbDataReader reader);
        List<T> Map(System.Data.DataTable table);
    }
}
