using System;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlTool
{
    public class CodeGeneration
    {
        public static string GetCrudProcs(
            string serverName,
            string databaseName,
            TableInfo tableInfo
        )
        {
            Table table = SqlSchema.GetTable(serverName, databaseName, tableInfo.SchemaName, tableInfo.TableName);
            if (!HasPrimaryKey(table))
            {
                throw new ApplicationException("Table does not have a primary key");
            }
            var returnSql = new StringBuilder(); ;
            returnSql.AppendLine(GetSelectProc(table));
            returnSql.AppendLine(GetDeleteProc(table));
            returnSql.AppendLine(GetInsertProc(table));
            returnSql.AppendLine(GetUpdateProc(table));
            return returnSql.ToString();
        }

        private static string GetSelectProc(
            Table table
        )
        {
            var returnSql = new StringBuilder();
            string procName = table.Schema + ".Get" + table.Name;
            returnSql.AppendLine(GetDropObjectSQL(procName, "procedure"));
            returnSql.AppendLine("create procedure " + procName + "(");
            returnSql.AppendLine(GetPrimaryKeyInputParameters(table));
            returnSql.AppendLine(")");
            returnSql.AppendLine("as");
            returnSql.AppendLine("select " + SqlSchema.GetColumnListString(table, true));
            returnSql.AppendLine("from " + table.Schema + "." + table.Name);
            returnSql.AppendLine(GetPrimaryKeyWhereClause(table) + ";");
            returnSql.AppendLine("go");
            return returnSql.ToString();
        }

        private static string GetDeleteProc(
            Table table
        )
        {
            string columnList = "";
            foreach (Column column in table.Columns)
            {
                if (columnList.Length > 0)
                {
                    columnList += ", ";
                }
                columnList += column.Name;
            }
            var returnSql = new StringBuilder();
            string procName = table.Schema + ".Delete" + table.Name;
            returnSql.AppendLine(GetDropObjectSQL(procName, "procedure"));
            returnSql.AppendLine("create procedure " + procName + "(");
            returnSql.AppendLine(GetPrimaryKeyInputParameters(table));
            returnSql.AppendLine(")");
            returnSql.AppendLine("as");
            returnSql.AppendLine("delete");
            returnSql.AppendLine("from " + table.Schema + "." + table.Name);
            returnSql.AppendLine(GetPrimaryKeyWhereClause(table) + ";");
            returnSql.AppendLine("go");
            return returnSql.ToString();
        }

        private static string GetInsertProc(
            Table table
        )
        {
            var returnSql = new StringBuilder();
            string procName = table.Schema + ".Insert" + table.Name;
            returnSql.AppendLine(GetDropObjectSQL(procName, "procedure"));
            returnSql.AppendLine("create procedure " + procName + "(");
            returnSql.AppendLine(GetColumnParameterString(table, true, false));
            returnSql.AppendLine(")");
            returnSql.AppendLine("as");
            returnSql.Append("insert into " + table.Schema + "." + table.Name + "(");
            returnSql.AppendLine(SqlSchema.GetColumnListString(table, false) + ")");
            returnSql.AppendLine("values(" + GetColumnParameterString(table, false, false) + ");");
            returnSql.AppendLine("go");
            return returnSql.ToString();
        }

        private static string GetUpdateProc(
            Table table
        )
        {
            var returnSql = new StringBuilder();
            string procName = table.Schema + ".Update" + table.Name;
            returnSql.AppendLine(GetDropObjectSQL(procName, "procedure"));
            returnSql.AppendLine("create procedure " + procName + "(");
            returnSql.AppendLine(GetColumnParameterString(table, true, true));
            returnSql.AppendLine(")");
            returnSql.AppendLine("as");
            returnSql.AppendLine("update " + table.Schema + "." + table.Name);
            returnSql.AppendLine("set ");
            string columnList = "";
            foreach (Column column in table.Columns)
            {
                if (!column.InPrimaryKey)
                {
                    if (columnList.Length > 0)
                    {
                        columnList += ", " + Environment.NewLine;
                    }
                    columnList += column.Name + " = @" + column.Name;
                }
            }
            returnSql.AppendLine(columnList);
            returnSql.AppendLine(GetPrimaryKeyWhereClause(table) + ";");
            returnSql.AppendLine("go");
            return returnSql.ToString();
        }

        private static string GetDropObjectSQL(
            string objectName,
            string objectType
        )
        {
            var returnSql = "if(object_id('" + objectName + "') is not null)" + Environment.NewLine;
            returnSql += "\tdrop " + objectType + " " + objectName + ";" + Environment.NewLine;
            returnSql += "go";
            return returnSql;
        }

        private static List<Column> GetPrimaryKeyColumns(
            Table table
        )
        {
            List<Column> returnList = new List<Column>();
            foreach (Column column in table.Columns)
            {
                if (column.InPrimaryKey)
                {
                    returnList.Add(column);
                }
            }
            return returnList;
        }

        private static string GetPrimaryKeyWhereClause(
            Table table
        )
        {
            string returnString = "";
            List<Column> pkColumns = GetPrimaryKeyColumns(table);
            foreach(Column column in pkColumns)
            {
                if (returnString.Length > 0)
                {
                    returnString += Environment.NewLine + " and ";
                }
                returnString += column.Name + " = @" + column.Name;
            }
            return "where " + returnString;
        }

        private static bool HasPrimaryKey(
            Table table
        )
        {
            foreach (Column column in table.Columns)
            {
                if (column.InPrimaryKey)
                {
                    return true;
                }
            }
            return false;
        }

        private static string GetPrimaryKeyInputParameters(
            Table table
        )
        {
            string returnString = "";
            List<Column> pkColumns = GetPrimaryKeyColumns(table);
            foreach (Column column in pkColumns)
            {
                if (returnString.Length > 0)
                {
                    returnString += ", " + Environment.NewLine;
                }
                returnString += "@" + column.Name + " "  + SqlSchema.GetSqlDataType(column);
            }
            return returnString;
        }
        
        private static string GetColumnParameterString(
            Table table,
            bool includeDataType,
            bool includeIdentityColumn
        )
        {
            string returnString = "";
            foreach (Column column in table.Columns)
            {
                if (!column.Identity || includeIdentityColumn)
                {
                    if (returnString.Length > 0)
                    {
                        returnString += ", " + Environment.NewLine;
                    }
                    returnString += "@" + column.Name;
                    if (includeDataType)
                    {
                        returnString += " " + SqlSchema.GetSqlDataType(column);
                    }
                }
            }
            return returnString;
        }

    }
}
