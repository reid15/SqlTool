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
            string connectionString = GetConnectionString(serverName, databaseName);
            string fullTableName = tableInfo.SchemaName + "." + tableInfo.TableName;
            if (whereClause.Length > 0 && !whereClause.ToLower().StartsWith("where"))
            {
                whereClause = "where " + whereClause;
            }
            string sql = "select * from " + fullTableName + " "  + whereClause + ";";
            DataTable dt = FillDataTable(sql, connectionString);
            var table = SqlSchema.GetTable(serverName, databaseName, tableInfo.SchemaName, tableInfo.TableName);
            string columnList = SqlSchema.GetColumnListString(table, true);
            var columns = SqlSchema.GetColumnList(table);
            string insertLine = "insert into " + fullTableName + "(";
            insertLine += columnList + ")" + Environment.NewLine;
            insertLine += "values({0});";
            var returnSql = new StringBuilder();
            bool hasIdentity = SqlSchema.HasIdentityColumn(table);
            if (hasIdentity)
            {
                returnSql.AppendLine("set identity_insert " + fullTableName + " on; ");
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
                returnSql.AppendLine("set identity_insert " + fullTableName + " off;");
            }
            return returnSql.ToString();
        }

        private static string GetScriptedRow(
            DataRow row,
            List<Column> columnList
        )
        {
            var returnSql = new StringBuilder();
            foreach (var item in columnList)
            {
                if (returnSql.Length > 0)
                {
                    returnSql.Append(", ");
                }
                returnSql.Append(GetScriptedValue(row[item.Name], item.DataType.Name));
            }
            return returnSql.ToString();
        }

        private static string GetScriptedValue (
            object itemValue,
            string columnDataType
        )
        {
            if (itemValue == DBNull.Value)
            {
                return "NULL";
            }
            if (columnDataType == "bit")
            {
                if ((bool)itemValue)
                {
                    return "1";
                }
                return "0";
            }
            if (columnDataType == "int" || columnDataType == "decimal" || columnDataType == "smallint" ||
                columnDataType == "tinyint" || columnDataType == "bit")
            {
                return itemValue.ToString();
            }
            if (columnDataType == "binary" || columnDataType == "varbinary")
            {
                var binaryArray = (byte[])itemValue;
                var returnString = new StringBuilder();
                returnString.Append("0x");
                foreach (var item in binaryArray)
                {
                    returnString.Append(item.ToString("X2"));
                }
                return returnString.ToString();
            }
            return "'" + EscapeStringForSql(itemValue.ToString()) + "'";
        }

        private static string EscapeStringForSql (
            string inputString
        )
        {
            return inputString.Replace("'", "''");
        }

        private static string GetConnectionString(
            string serverName,
            string databaseName
        )
        {
            string connectionString = "server=" + serverName + ";database=" + databaseName + ";";
            connectionString += "integrated security=sspi;";
            return connectionString;
        }
                
        private static DataTable FillDataTable(
            string sql,
            string connectionString
        )
        {
            DataSet returnDataset = new DataSet();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    dataAdapter.Fill(returnDataset);
                }
            }
            return returnDataset.Tables[0];
        }

    }
}

