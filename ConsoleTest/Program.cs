using DataMapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DataTable testTable = new DataTable();
            testTable.Columns.Add("key", typeof(int));
            testTable.Columns.Add("value", typeof(string));

            DataRow testRow = testTable.NewRow();
            testRow["key"] = 0;
            testRow["value"] = "value at 0";
            testTable.Rows.Add(testRow);

            testRow = testTable.NewRow();
            testRow["key"] = 1;
            testRow["value"] = "value at 1";
            testTable.Rows.Add(testRow);

            testRow = testTable.NewRow();
            testRow["key"] = 2;
            testRow["value"] = "value at 2";
            testTable.Rows.Add(testRow);

            testTable.AcceptChanges();

            SqlMapper<testType> myMapper = new SqlMapper<testType>();
            myMapper.Map(t => t.key);
            myMapper.Map(t => t.value);
            List<testType> testOutput = myMapper.Map(testTable);

            Console.Out.WriteLine(testOutput[0].key);
        }
    }

    public class testType
    {
        public int key { get; set; }
        public string value { get; set; }
    }
}
