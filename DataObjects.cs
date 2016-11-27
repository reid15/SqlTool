using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlTool
{
    public class TableInfo
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }

        public TableInfo(
            string schemaName,
            string tableName
        )
        {
            SchemaName = schemaName;
            TableName = tableName;
        }
        public override string ToString()
        {
            return SchemaName + "." + TableName;
        }
    }
        
}
