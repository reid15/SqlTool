using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SqlTool
{
    public class ScriptData
    {
        public static string GetScriptedData(
            string serverName,
            string databaseName,
            TableInfo tableInfo,
            string whereClause
        )
        {
            string connectionString = DatabaseCommon.DataAccess.GetConnectionString(serverName, databaseName);
            string fullTableName = tableInfo.SchemaName + "." + tableInfo.TableName;
            if (whereClause.Length > 0 && !whereClause.ToLower().StartsWith("where"))
            {
                whereClause = "where " + whereClause;
            }
            string sql = "select * from " + fullTableName + " "  + whereClause + ";";
            DataTable dt = DatabaseCommon.DataAccess.GetDataTable(connectionString, sql);
            var table = DatabaseCommon.DatabaseSchema.GetTable(serverName, databaseName, tableInfo.SchemaName, tableInfo.TableName);
            string columnList = SqlSchema.GetColumnListString(table, true);
            var columns = SqlSchema.GetColumnList(table);
            string insertLine = "INSERT INTO " + fullTableName + "(";
            insertLine += columnList + ")" + Environment.NewLine;
            insertLine += "VALUES({0});";
            var returnSql = new StringBuilder();
            bool hasIdentity = DatabaseCommon.DatabaseSchema.HasIdentityColumn(table);
            if (hasIdentity)
            {
                returnSql.AppendLine("SET IDENTITY_INSERT " + fullTableName + " ON; ");
                returnSql.AppendLine();
            }
            foreach(DataRow row in dt.Rows)
            {
                string scriptedRow = GetScriptedRow(row, columns);
                returnSql.AppendLine(String.Format(insertLine, scriptedRow));
                returnSql.AppendLine();
            }
            if (hasIdentity)
            {
                returnSql.AppendLine("SET IDENTITY_INSERT " + fullTableName + " OFF;");
            }
            return returnSql.ToString();
        }

        private static string GetScriptedRow(
            DataRow row,
            List<Column> columnList
        )
        {
            var returnSql = new StringBuilder();
            foreach (Column item in columnList)
            {
                if (returnSql.Length > 0)
                {
                    returnSql.Append(", ");
                }
                returnSql.Append(DatabaseCommon.ScriptData.DelimitData(row[item.Name], item));
            }
            return returnSql.ToString();
        }

    }
}

