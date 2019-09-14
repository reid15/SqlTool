using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;

namespace SqlTool
{
    public class SqlSchema
    {

        public static List<TableInfo>GetTableList(
            string serverName,
            string databaseName
        )
        {
            var database = DatabaseCommon.DatabaseSchema.GetDatabase(serverName, databaseName);
            var tableList = new List<TableInfo>();
            foreach (Table table in database.Tables)
            {
                tableList.Add(new TableInfo(table.Schema, table.Name));
            }
            return tableList;
        }

        public static List<Column> GetColumnList(
            Table table
        )
        {
            List<Column> columnList = new List<Column>();
            foreach (Column column in table.Columns)
            {
                columnList.Add(column);
            }
            return columnList;
        }

        public static Column GetIdentityColumn(
            Table table
        )
        {
            foreach (Column column in table.Columns)
            {
                if (column.Identity)
                {
                    return column;
                }
            }
            return new Column();
        }

        public static string GetColumnListString(
            Table table,
            bool includeIdentityColumn,
            string columnPrefix = ""
        )
        {
            string columnList = "";
            foreach (Column column in table.Columns)
            {
                if (!column.Identity || includeIdentityColumn)
                {
                    if (columnList.Length > 0)
                    {
                        columnList += ", ";
                    }
                    if (columnPrefix.Length > 0)
                    {
                        columnList += columnPrefix + ".";
                    }
                    columnList += column.Name;
                }
            }
            return columnList;
        }

        public static string GetColumnDefaultValue(
            Column column
        )
        {
            if (column.DefaultConstraint == null)
            {
                return "";
            }
            return column.DefaultConstraint.Text;
        }

        public static List<ForeignKey> GetForeignKeys(
            Database database
        )
        {
            var returnList = new List<ForeignKey>();
            foreach (Table table in database.Tables)
            {
                if (!table.IsSystemObject)
                {
                    foreach (ForeignKey foreignKey in table.ForeignKeys)
                    {
                        returnList.Add(foreignKey);
                    }
                }
            }
            return returnList;
        }
    }
}
