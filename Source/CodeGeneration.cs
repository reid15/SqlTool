using System;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;
using System.Text;

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
            Table table = DatabaseCommon.DatabaseSchema.GetTable(serverName, databaseName, tableInfo.SchemaName, tableInfo.TableName);
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
            returnSql.AppendLine(GetDropObjectSQL(procName, "PROCEDURE"));
            returnSql.AppendLine("CREATE PROCEDURE " + procName + "(");
            returnSql.AppendLine(GetSelectProcParameters(table));
            returnSql.AppendLine(")");
            returnSql.AppendLine("AS");
            returnSql.AppendLine("SELECT");
            returnSql.AppendLine(GetNonPrimaryKeyOutputParameters(table));
            returnSql.AppendLine("FROM " + table.Schema + "." + table.Name);
            returnSql.AppendLine(GetPrimaryKeyWhereClause(table) + ";");
            returnSql.AppendLine("GO");
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
            returnSql.AppendLine(GetDropObjectSQL(procName, "PROCEDURE"));
            returnSql.AppendLine("CREATE PROCEDURE " + procName + "(");
            returnSql.AppendLine(GetPrimaryKeyInputParameters(table));
            returnSql.AppendLine(")");
            returnSql.AppendLine("AS");
            returnSql.AppendLine("DELETE");
            returnSql.AppendLine("FROM " + table.Schema + "." + table.Name);
            returnSql.AppendLine(GetPrimaryKeyWhereClause(table) + ";");
            returnSql.AppendLine("GO");
            return returnSql.ToString();
        }

        private static string GetInsertProc(
            Table table
        )
        {
            var returnSql = new StringBuilder();
            string procName = table.Schema + ".Insert" + table.Name;
            Column identityColumn = SqlSchema.GetIdentityColumn(table);
            returnSql.AppendLine(GetDropObjectSQL(procName, "PROCEDURE"));
            returnSql.AppendLine("CREATE PROCEDURE " + procName + "(");
            if (identityColumn.Name != null)
            {
                returnSql.AppendLine("@" + identityColumn.Name + " " + DatabaseCommon.DatabaseSchema.GetColumnDataType(identityColumn) + " output");
            }
            returnSql.AppendLine(GetColumnParameterString(table, true, false));
            returnSql.AppendLine(")");
            returnSql.AppendLine("AS");
            returnSql.Append("INSERT INTO " + table.Schema + "." + table.Name + "(");
            returnSql.AppendLine(SqlSchema.GetColumnListString(table, false) + ")");
            returnSql.AppendLine("VALUES(" + GetColumnParameterString(table, false, false) + ");");
            if (identityColumn.Name != null)
            {
                returnSql.AppendLine("SET @" + identityColumn.Name + " = Scope_Identity();");
            }
            returnSql.AppendLine();
            returnSql.AppendLine("GO");
            return returnSql.ToString();
        }

        private static string GetUpdateProc(
            Table table
        )
        {
            var returnSql = new StringBuilder();
            string procName = table.Schema + ".Update" + table.Name;
            returnSql.AppendLine(GetDropObjectSQL(procName, "PROCEDURE"));
            returnSql.AppendLine("CREATE PROCEDURE " + procName + "(");
            returnSql.AppendLine(GetColumnParameterString(table, true, true));
            returnSql.AppendLine(")");
            returnSql.AppendLine("AS");
            returnSql.AppendLine("UPDATE " + table.Schema + "." + table.Name);
            returnSql.AppendLine("SET ");
            returnSql.AppendLine(GetUpdateWithParameter(table));
            returnSql.AppendLine(GetPrimaryKeyWhereClause(table) + ";");
            returnSql.AppendLine("GO");
            return returnSql.ToString();
        }

        private static string GetUpdateWithParameter(Table table)
        {
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
            return columnList;
        }

        private static string GetDropObjectSQL(
            string objectName,
            string objectType
        )
        {
            var returnSql = "IF(OBJECT_ID('" + objectName + "') IS NOT NULL)" + Environment.NewLine;
            returnSql += "\tDROP " + objectType + " " + objectName + ";" + Environment.NewLine;
            returnSql += "GO";
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
            foreach (Column column in pkColumns)
            {
                if (returnString.Length > 0)
                {
                    returnString += Environment.NewLine + " AND ";
                }
                returnString += column.Name + " = @" + column.Name;
            }
            return "WHERE " + returnString;
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
                returnString += "@" + column.Name + " " + DatabaseCommon.DatabaseSchema.GetColumnDataType(column);
            }
            return returnString;
        }

        private static string GetSelectProcParameters(
            Table table
        )
        {
            string columnList = "";
            foreach (Column column in table.Columns)
            {
                if (columnList.Length > 0)
                {
                    columnList += ", " + Environment.NewLine;
                }
                columnList += "@" + column.Name + " " + DatabaseCommon.DatabaseSchema.GetColumnDataType(column);
                if (!column.InPrimaryKey)
                {
                    columnList += " output";
                }
            }

            return columnList;
        }

        private static string GetNonPrimaryKeyOutputParameters(
            Table table
        )
        {
            string columnList = "";
            foreach (Column column in table.Columns)
            {
                if (!column.InPrimaryKey)
                {
                    if (columnList.Length > 0)
                    {
                        columnList += ", " + Environment.NewLine;
                    }
                    columnList += "@" + column.Name + " = " + column.Name;
                }
            }

            return columnList;
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
                        returnString += " " + DatabaseCommon.DatabaseSchema.GetColumnDataType(column);
                    }
                }
            }
            return returnString;
        }

        public static string GetMergeProc(
            string serverName,
            string databaseName,
            TableInfo tableInfo
        )
        {
            string sourceAlias = "Source";
            string targetAlias = "Target";
            Table table = DatabaseCommon.DatabaseSchema.GetTable(serverName, databaseName, tableInfo.SchemaName, tableInfo.TableName);
            if (!HasPrimaryKey(table))
            {
                throw new ApplicationException("Table does not have a primary key");
            }
            string procName = tableInfo.SchemaName + ".Merge" + tableInfo.TableName;
            var returnSql = new StringBuilder();
            returnSql.AppendLine(GetDropObjectSQL(procName, "PROCEDURE"));
            returnSql.AppendLine("CREATE PROCEDURE " + procName);
            returnSql.AppendLine(GetColumnParameterString(table, true, true));
            returnSql.AppendLine("AS");
            returnSql.AppendLine("MERGE " + tableInfo.SchemaName + "." + tableInfo.TableName + " AS " + targetAlias);
            returnSql.AppendLine("USING (SELECT");
            returnSql.AppendLine(GetSelectColumnParameters(table));
            returnSql.AppendLine(") AS " + sourceAlias);
            returnSql.AppendLine("ON");
            returnSql.AppendLine(GetJoinForPrimaryKey(table, sourceAlias, targetAlias));
            returnSql.AppendLine(GetMergeUpdateSQL(table, sourceAlias, targetAlias).TrimEnd());
            returnSql.AppendLine("WHEN NOT MATCHED THEN INSERT (");
            returnSql.AppendLine(SqlSchema.GetColumnListString(table, false));
            returnSql.AppendLine(") VALUES (");
            returnSql.AppendLine(SqlSchema.GetColumnListString(table, false, sourceAlias));
            returnSql.AppendLine(");");

            return returnSql.ToString();
        }

        private static string GetSelectColumnParameters(Table table)
        {
            string columnList = "";
            foreach (Column column in table.Columns)
            {
                if (columnList.Length > 0)
                {
                    columnList += ", " + Environment.NewLine;
                }
                columnList += "@" + column.Name + " AS " + column.Name;
            }

            return columnList;
        }

        private static string GetJoinForPrimaryKey(
            Table table,
            string sourceTableName,
            string targetTableName
        )
        {
            string returnString = "";
            var pk = GetPrimaryKeyColumns(table);
            foreach(var item in pk)
            {
                if (returnString.Length > 0)
                {
                    returnString += " AND " + Environment.NewLine;
                }
                returnString += targetTableName + "." + item + " = " + sourceTableName + "." + item;
            }

            return returnString;
        }
        
        private static string GetMergeUpdateSQL(
            Table table,
            string sourceAlias,
            string targetAlias
        )
        {
            string compareList = "";
            string columnSetList = "";
            foreach (Column column in table.Columns)
            {
                if (!column.InPrimaryKey)
                {
                    if (columnSetList.Length > 0)
                    {
                        columnSetList += ", " + Environment.NewLine;
                        compareList += " OR " + Environment.NewLine;
                    }
                    var defaultValue = GetDefaultForDataType(column);

                    compareList += "ISNULL(" + targetAlias + "." + column.Name + "," + defaultValue + ") <> ISNULL(" + sourceAlias + "." + column.Name + ", " + defaultValue + ")";
                    columnSetList += column.Name + " = " + sourceAlias + "." + column.Name;
                }
            }

            var returnSql = new StringBuilder();
            returnSql.AppendLine("WHEN MATCHED AND (");
            returnSql.AppendLine(compareList);
            returnSql.AppendLine(") THEN UPDATE SET");
            returnSql.AppendLine(columnSetList);

            return returnSql.ToString();
        }

        private static string GetDefaultForDataType(Column column)
        {
            if(DatabaseCommon.DatabaseSchema.IsNumericDataType(column))
            {
                return "-1";
            }
            switch(column.DataType.Name)
            {
                case "date":
                case "datetime":
                    return "'2000-01-01'";
                case "bit":
                    return "-1";
            }
            return "''";
        }
    }
}
