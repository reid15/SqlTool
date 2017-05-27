using System;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;

namespace SqlTool
{
    public class SqlSchema
    {
        public static Database GetDatabase(
            string serverName,
            string databaseName
        )
        {
            Server server = new Server(serverName);
            if (server == null)
            {
                throw new ApplicationException("Server not found");
            }
            var database = server.Databases[databaseName];
            if (database == null)
            {
                throw new ApplicationException("Database not found");
            }
            return database;
        }

        public static List<TableInfo>GetTableList(
            string serverName,
            string databaseName
        )
        {
            var database = GetDatabase(serverName, databaseName);
            var tableList = new List<TableInfo>();
            foreach (Table table in database.Tables)
            {
                tableList.Add(new TableInfo(table.Schema, table.Name));
            }
            return tableList;
        }

        public static Table GetTable(
            string serverName,
            string databaseName,
            string schemaName,
            string tableName
        )
        {
            var database = GetDatabase(serverName, databaseName);
            return GetTable(database, schemaName, tableName);
        }

        public static Table GetTable(
            Database database,
            string schemaName,
            string tableName
        )
        {
            var table = database.Tables[tableName, schemaName];
            if (table == null)
            {
                throw new ApplicationException("Table not found");
            }
            return table;
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

        public static bool HasIdentityColumn(
            Table table
        )
        {
            List<Column> columnList = new List<Column>();
            foreach (Column column in table.Columns)
            {
                if (column.Identity)
                {
                    return true;
                }
            }
            return false;
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
            bool includeIdentityColumn
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
                    columnList += column.Name;
                }
            }
            return columnList;
        }

        public static string GetSqlDataType(
            Column column
        )
        {
            string returnType = "";
            switch (column.DataType.Name)
            {
                case "varchar":
                case "char":
                case "nvarchar":
                case "nchar":
                case "binary":
                case "varbinary":
                    string length = column.DataType.MaximumLength.ToString();
                    if (length == "-1")
                    {
                        length = "max";
                    }
                    returnType = column.DataType.Name + "(" + length + ")";
                    break;
                case "decimal":
                    returnType = column.DataType.Name + "(" + column.DataType.NumericPrecision.ToString() + "," +
                        column.DataType.NumericScale + ")";
                    break;
                default:
                    returnType = column.DataType.Name;
                    break;
            }
            return returnType;
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
