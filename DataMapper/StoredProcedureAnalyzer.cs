using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandBuilder
{
    public static class StoredProcedureAnalyzer
    {
        private static Dictionary<SqlDbType, Type> _typeMappings = new Dictionary<SqlDbType, Type>()
        {
            {SqlDbType.BigInt, typeof(Int64)},
            {SqlDbType.Binary, typeof(Byte[])},
            {SqlDbType.Bit, typeof(Boolean) },
            {SqlDbType.Char, typeof(String) },
            {SqlDbType.Date, typeof(DateTime) },
            {SqlDbType.DateTime, typeof(DateTime) },
            {SqlDbType.DateTime2, typeof(DateTime) },
            {SqlDbType.DateTimeOffset, typeof(DateTimeOffset) },
            {SqlDbType.Decimal, typeof(Decimal) },
            {SqlDbType.VarBinary, typeof(Byte[]) },
            {SqlDbType.Float, typeof(Double) },
            {SqlDbType.Int, typeof(Int32) },
            {SqlDbType.Money, typeof(Decimal) },
            {SqlDbType.NChar, typeof(String) },
            {SqlDbType.NText, typeof(String) },
            {SqlDbType.NVarChar, typeof(String) },
            {SqlDbType.Real, typeof(Single) },
            {SqlDbType.Timestamp, typeof(byte[]) },
            {SqlDbType.SmallDateTime, typeof(DateTime) },
            {SqlDbType.SmallInt, typeof(Int16) },
            {SqlDbType.SmallMoney, typeof(Decimal) },
            {SqlDbType.Variant, typeof(Object) },
            {SqlDbType.Text, typeof(String) },
            {SqlDbType.Time, typeof(TimeSpan) },
            {SqlDbType.TinyInt, typeof(Byte) },
            {SqlDbType.UniqueIdentifier, typeof(Guid) },
            {SqlDbType.VarChar, typeof(String) },
            {SqlDbType.Xml, typeof(String) }
        };

        private static DataMapper.SqlMapper<ReturnSchema> SchemaMap = new DataMapper.SqlMapper<ReturnSchema>(true);

        //https://stackoverflow.com/questions/33761/how-can-i-retrieve-a-list-of-parameters-from-a-stored-procedure-in-sql-server
        //https://stackoverflow.com/questions/3038364/get-stored-procedure-parameters-by-either-c-sharp-or-sql
        //SELECT * FROM sys.dm_exec_describe_first_result_set ('owner.sprocName', NULL, 0) ;
        //Select * from sys.objects where type = 'p' and name = (procedure name)
        //cmd.Execute(CommandBehaviour.SchemaOnly)
        public static CommandDescriptor GetParameters(string sqlCommand, string sqlConnection)
        {
            CommandDescriptor description = new CommandDescriptor() { parameters = null, tableValues = null };

            using (SqlConnection conn = new SqlConnection(sqlConnection))
            using(SqlCommand cmd = new SqlCommand(sqlCommand, conn) { CommandType = CommandType.StoredProcedure })
            {
                DataTable schemaTable = null;
                try
                {
                    conn.Open();
                    SqlCommandBuilder.DeriveParameters(cmd);

                    if (cmd.Parameters != null && cmd.Parameters.Count > 0)
                    {
                        description.parameters = new List<SqlParameter>();
                        foreach (SqlParameter parameter in cmd.Parameters)
                            description.parameters.Add(parameter);
                    }

                    using (SqlDataReader schemaReader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        schemaTable = schemaReader.GetSchemaTable();
                    }
                }
                finally
                {
                    conn.Close();
                }
                

                if(schemaTable != null)
                {
                    description.returnSchema = SchemaMap.Map(schemaTable);
                }
            }

            return description;
        }

        public class CommandDescriptor
        {
            public List<SqlParameter> parameters { get; set; }
            public List<ReturnSchema> returnSchema { get; set; }

            #region parameter helpers
            public string GetMethodParameters()
            {
                if(parameters == null || parameters.Count == 0)
                {
                    return string.Empty;
                }

                return string.Join(",",
                        parameters.Select(param => _typeMappings[param.SqlDbType].ToString() + " " + param.ParameterName.Replace("@", ""))
                    );
            }

            public string GetCmdParameters()
            {
                if(parameters == null || parameters.Count == 0)
                {
                    return string.Empty;
                }

                return string.Join(",",
                        parameters.Select(param => param.ParameterName.Replace("@", "") + "SqlParam")
                    );
            }

            public List<string> BuildSqlParameters(int tabLevel = 0)
            {
                if (parameters == null || parameters.Count == 0)
                    return null;

                List<string> sqlParameterList = new List<string>();
                string spacing = "".PadLeft(tabLevel * 5);
                
                foreach(var parameter in parameters)
                {
                    StringBuilder paramBuilder = new StringBuilder();

                    paramBuilder.AppendFormat("{2}System.Data.SqlClient.SqlParameter {0}SqlParam = new System.Data.SqlClient.SqlParameter(\"@{0}\", {1}) { Value = {0} };", 
                                                (parameter.ParameterName.Replace("@", "")),
                                                DbTypeString(parameter.SqlDbType),
                                                spacing);
                    paramBuilder.AppendLine();
                    paramBuilder.AppendFormat("{1}if ({0} == null) {0}SqlParam.Value = System.DBNull.Value;", 
                                              parameter.ParameterName.Replace("@", ""), 
                                              spacing);
                    
                    sqlParameterList.Add(paramBuilder.ToString());
                }

                return sqlParameterList;
            }
            #endregion

            private static string DbTypeString(SqlDbType type)
            {
                string baseString = "System.Data.SqlDbType.";
                switch (type)
                {
                    case SqlDbType.BigInt:
                        return baseString + "BigInt";
                    case SqlDbType.Binary:
                        return baseString + "Binary";
                    case SqlDbType.Bit:
                        return baseString + "Bit";
                    case SqlDbType.Char:
                        return baseString + "Char";
                    case SqlDbType.Date:
                        return baseString + "Date";
                    case SqlDbType.DateTime:
                        return baseString + "DateTime";
                    case SqlDbType.DateTime2:
                        return baseString + "DateTime2";
                    case SqlDbType.DateTimeOffset:
                        return baseString + "DateTimeOffset";
                    case SqlDbType.Decimal:
                        return baseString + "Decimal";
                    case SqlDbType.Float:
                        return baseString + "Float";
                    case SqlDbType.Image:
                        return baseString + "Image";
                    case SqlDbType.Int:
                        return baseString + "Int";
                    case SqlDbType.Money:
                        return baseString + "Money";
                    case SqlDbType.NChar:
                        return baseString + "NChar";
                    case SqlDbType.NText:
                        return baseString + "NText";
                    case SqlDbType.NVarChar:
                        return baseString + "NVarChar";
                    case SqlDbType.Real:
                        return baseString + "Real";
                    case SqlDbType.SmallDateTime:
                        return baseString + "SmallDateTime";
                    case SqlDbType.SmallInt:
                        return baseString + "SmallInt";
                    case SqlDbType.SmallMoney:
                        return baseString + "SmallMoney";
                    case SqlDbType.Structured:
                        return baseString + "Structured";
                    case SqlDbType.Text:
                        return baseString + "Text";
                    case SqlDbType.Time:
                        return baseString + "Time";
                    case SqlDbType.Timestamp:
                        return baseString + "Timestamp";
                    case SqlDbType.TinyInt:
                        return baseString + "TinyInt";
                    case SqlDbType.Udt:
                        return baseString + "Udt";
                    case SqlDbType.UniqueIdentifier:
                        return baseString + "UniqueIdentifier";
                    case SqlDbType.VarBinary:
                        return baseString + "VarBinary";
                    case SqlDbType.VarChar:
                        return baseString + "VarChar";
                    case SqlDbType.Variant:
                        return baseString + "Variant";
                    case SqlDbType.Xml:
                        return baseString + "Xml";
                }
                return "";
            }
        }

        public class ReturnSchema
        {
            public bool AllowDBNull { get; set; }
            public string ColumnName { get; set; }
            public SqlDbType NonVersionedProviderType { get; set; }
        }
    }
}
