using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlTool
{
    public class GenerateDataLayer
    {
        private Dictionary<string, bool> propertyNullability = new Dictionary<string, bool>();

        public string GetDataLayer(
            string serverName,
            string databaseName,
            string schemaName,
            string tableName
        )
        {
            var database = SqlSchema.GetDatabase(serverName, databaseName);
            var table = SqlSchema.GetTable(database, schemaName, tableName);
            SetPropertyNullability(table);

            var returnCode = new StringBuilder();
            returnCode.AppendLine("using System;");
            returnCode.AppendLine("using System.Data;");
            returnCode.AppendLine("using System.Data.SqlClient;");
            returnCode.AppendLine();
            returnCode.AppendLine(GetDataObject(table));
            returnCode.AppendLine(GetStoredProcedureCalls(database, table, schemaName));

            return returnCode.ToString();
        }

        private void SetPropertyNullability(Table table)
        {
            foreach (Column column in table.Columns)
            {
                propertyNullability.Add(column.Name, column.Nullable);
            }
        }

        private bool GetPropertyNullability(string propertyName)
        {
            bool isNullable;
            propertyNullability.TryGetValue(propertyName, out isNullable);
            return isNullable;
        }

        private string GetDataObject(Table table)
        {
            var returnCode = new StringBuilder();
            returnCode.AppendLine("public class " + table.Name);
            returnCode.AppendLine("{");
            foreach (Column column in table.Columns)
            {
                returnCode.AppendLine("\tpublic " + GetCSharpDataTypeNullable(column.DataType.SqlDataType, column.Nullable) + 
                    " " + column.Name + " { get; set; }");
            }
            returnCode.AppendLine("}");
            return returnCode.ToString();
        }

        private string GetStoredProcedureCalls(Database database, Table table, string schemaName)
        {
            var returnCode = new StringBuilder();
            returnCode.AppendLine("public class " + table.Name + "DataLayer");
            returnCode.AppendLine("{");

            // Insert Prod
            string procName =  "Insert" + table.Name;
            var proc = database.StoredProcedures[procName, schemaName];
            if (proc != null)
            {
                returnCode.AppendLine(GetNonQueryStoredProcedureCode(proc, table.Name, table));
            }

            // Update Prod
            procName = "Update" + table.Name;
            proc = database.StoredProcedures[procName, schemaName];
            if (proc != null)
            {
                returnCode.AppendLine(GetNonQueryStoredProcedureCode(proc, table.Name, table));
            }

            // Delete Prod
            procName = "Delete" + table.Name;
            proc = database.StoredProcedures[procName, schemaName];
            if (proc != null)
            {
                returnCode.AppendLine(GetNonQueryStoredProcedureCode(proc, table.Name, table));
            }

            // Get Prod
            procName = "Get" + table.Name;
            proc = database.StoredProcedures[procName, schemaName];
            if (proc != null)
            {
                returnCode.AppendLine(GetReaderStoredProcedureCode(proc, table.Name));
            }

            returnCode.AppendLine("}");
            return returnCode.ToString(); ;
        }

        private string GetNonQueryStoredProcedureCode(
            StoredProcedure storedProcedure, 
            string dataObjectType,
            Table table
        )
        {
            string dataObjectName = "dataObject";
            string commandObjectName = "cmd";
            StringBuilder returnCode = new StringBuilder();

            returnCode.AppendLine("public static void " + storedProcedure.Name + "(");

            returnCode.AppendLine("\t" + dataObjectType + " " + dataObjectName + ",");
            returnCode.AppendLine("\t" + "string connectionString");

            returnCode.AppendLine(")");
            returnCode.AppendLine("{");

            returnCode.AppendLine("using (SqlConnection con = new SqlConnection(connectionString)){");
            returnCode.AppendLine("using (SqlCommand " + commandObjectName + " = new SqlCommand(\"" + storedProcedure.Schema + "." + storedProcedure.Name + "\", con))");
            returnCode.AppendLine("{");
            returnCode.AppendLine(commandObjectName + ".CommandType = CommandType.StoredProcedure;");

            returnCode.AppendLine(GetStoredProcedureInputParameterCode(storedProcedure, dataObjectName));
            returnCode.AppendLine(GetStoredProcedureOutputParameterCode(storedProcedure));

            returnCode.AppendLine("con.Open();");
            returnCode.AppendLine(commandObjectName + ".ExecuteNonQuery();");

            returnCode.AppendLine(GetStoredProcedureOutputParameterToDataObjectCode(storedProcedure, dataObjectName, commandObjectName));

            returnCode.AppendLine("}");
            returnCode.AppendLine("}");
            returnCode.AppendLine("}");
            returnCode.AppendLine();

            return returnCode.ToString();
        }

        private string GetReaderStoredProcedureCode(
            StoredProcedure storedProcedure,
            string dataObjectName
        )
        {
            string returnDataObjectName = "dataObject";
            string commandObjectName = "cmd";

            StringBuilder returnCode = new StringBuilder();

            returnCode.AppendLine("public static " + dataObjectName + " " + storedProcedure.Name + "(");

            returnCode.Append(GetCSharpParameterCode(storedProcedure));
            returnCode.AppendLine("\t" + "string connectionString");

            returnCode.AppendLine(")");
            returnCode.AppendLine("{");

            returnCode.AppendLine(dataObjectName + " " + returnDataObjectName + " = new " + dataObjectName + "();");
            returnCode.AppendLine("using (SqlConnection con = new SqlConnection(connectionString)){");
            returnCode.AppendLine("using (SqlCommand " + commandObjectName + " = new SqlCommand(\"" + storedProcedure.Schema + "." + storedProcedure.Name + "\", con))");
            returnCode.AppendLine("{");
            returnCode.AppendLine(commandObjectName + ".CommandType = CommandType.StoredProcedure;");

            returnCode.AppendLine(GetStoredProcedureInputParameterCode(storedProcedure));
            returnCode.AppendLine(GetStoredProcedureOutputParameterCode(storedProcedure));

            returnCode.AppendLine("con.Open();");
            returnCode.AppendLine(commandObjectName + ".ExecuteNonQuery();");
            returnCode.AppendLine();

            returnCode.AppendLine(GetInputParameterParameterDataObjectCode(storedProcedure, returnDataObjectName));
            returnCode.AppendLine(GetStoredProcedureOutputParameterToDataObjectCode(storedProcedure, returnDataObjectName, commandObjectName));

            returnCode.AppendLine("}");
            returnCode.AppendLine("}");
            returnCode.AppendLine("return " + returnDataObjectName + ";");

            returnCode.AppendLine("}");
            returnCode.AppendLine();

            return returnCode.ToString();
        }

        private string GetStoredProcedureInputParameterCode(StoredProcedure storedProcedure)
        {
            StringBuilder returnCode = new StringBuilder();
            foreach (StoredProcedureParameter parameter in storedProcedure.Parameters)
            {
                if (!parameter.IsOutputParameter)
                {
                    string parameterName = parameter.ToString().Replace("[", "").Replace("]", "");
                    returnCode.AppendLine("cmd.Parameters.AddWithValue(\"" + parameterName + "\", " + 
                        parameterName.Replace("@", "") + ");");
                }
            }

            return returnCode.ToString();
        }

        private string GetStoredProcedureInputParameterCode(StoredProcedure storedProcedure, string dataObjectName)
        {
            if (dataObjectName.Length > 0)
            {
                dataObjectName += ".";
            }
            StringBuilder returnCode = new StringBuilder();
            foreach (StoredProcedureParameter parameter in storedProcedure.Parameters)
            {
                if (!parameter.IsOutputParameter)
                { 
                    string parameterName = parameter.ToString().Replace("[", "").Replace("]", "");
                    string propertyName = parameterName.Replace("@", "");
                    returnCode.Append("cmd.Parameters.AddWithValue(\"" + parameterName + "\", (" + dataObjectName +
                        propertyName);
                    if (GetPropertyNullability(propertyName))
                    {
                        returnCode.Append(" ?? Convert.DBNull");
                    }
                    returnCode.AppendLine("));");
                }
            }

            return returnCode.ToString();
        }

        private string GetStoredProcedureOutputParameterCode(StoredProcedure storedProcedure)
        {
            StringBuilder returnCode = new StringBuilder();
            foreach (StoredProcedureParameter parameter in storedProcedure.Parameters)
            {
                if (parameter.IsOutputParameter)
                {
                    string parameterName = parameter.ToString().Replace("[", "").Replace("]", "");
                    int parameterSize = GetParameterSize(parameter);
                    returnCode.Append("cmd.Parameters.Add(\"" + parameterName + "\", " +
                        "SqlDbType." + GetSqlDbTypeString(parameter.DataType.SqlDataType));
                    if (parameterSize != 0)
                    {
                        returnCode.Append(", " + parameterSize.ToString());
                    }
                    returnCode.AppendLine(").Direction = ParameterDirection.Output;");
                }
            }

            return returnCode.ToString();
        }

        private int GetParameterSize(StoredProcedureParameter parameter)
        {
            if (parameter.DataType.IsStringType)
            {
                return parameter.DataType.MaximumLength;
            }
            if (parameter.DataType.SqlDataType == SqlDataType.VarBinary || parameter.DataType.SqlDataType == SqlDataType.VarBinaryMax)
            {
                return parameter.DataType.MaximumLength;
            }
            return 0;
        }

        private string GetSqlDbTypeString(SqlDataType sqlDataType)
        {
            switch(sqlDataType)
            {
                case SqlDataType.NVarCharMax:
                    return "NVarChar";
                case SqlDataType.VarCharMax:
                    return "VarChar";
                case SqlDataType.VarBinaryMax:
                    return "VarBinary";
                default:
                    return sqlDataType.ToString();
            }
        }

        private string GetStoredProcedureOutputParameterToDataObjectCode(
            StoredProcedure storedProcedure,
            string returnDataObjectName,
            string commandObjectName
        )
        {
            StringBuilder returnCode = new StringBuilder();
            foreach (StoredProcedureParameter parameter in storedProcedure.Parameters)
            {
                if (parameter.IsOutputParameter)
                {
                    string parameterName = parameter.ToString().Replace("[", "").Replace("]", "");
                    string propertyName = parameterName.Replace("@", "");
                    string parameterValue = commandObjectName + ".Parameters[\"" + parameterName + "\"].Value";
                    bool isNullable = GetPropertyNullability(propertyName);
                    returnCode.AppendLine(returnDataObjectName + "." + propertyName + " = " +
                        GetConvertToCode(parameter.DataType.SqlDataType, parameterValue, isNullable) + ";");
                }
            }

            return returnCode.ToString();
        }

        private string GetCSharpParameterCode(StoredProcedure storedProcedure)
        {
            StringBuilder returnCode = new StringBuilder();
            foreach (StoredProcedureParameter parameter in storedProcedure.Parameters)
            {
                if (!parameter.IsOutputParameter)
                {
                    string parameterName = parameter.ToString().Replace("[", "").Replace("]", "").Replace("@", "");
                    returnCode.AppendLine("\t" + GetCSharpDataType(parameter.DataType.SqlDataType) + " " + parameterName + ",");
                }
            }

            return returnCode.ToString();
        }

        private string GetInputParameterParameterDataObjectCode(StoredProcedure storedProcedure, string dataObjectName)
        {
            StringBuilder returnCode = new StringBuilder();
            foreach (StoredProcedureParameter parameter in storedProcedure.Parameters)
            {
                if (!parameter.IsOutputParameter)
                {
                    string parameterName = parameter.ToString().Replace("[", "").Replace("]", "");
                    string propertyName = parameterName.Replace("@", "");
                    returnCode.AppendLine(dataObjectName + "." + propertyName + " = " + propertyName +";");
                }
            }

            return returnCode.ToString();
        }

        private string GetCSharpDataType(
            SqlDataType sqlDataType
        )
        {
            switch (sqlDataType)
            {
                case SqlDataType.Binary:
                case SqlDataType.VarBinary:
                case SqlDataType.VarBinaryMax:
                    return "Byte[]"; //"String"
                case SqlDataType.Bit:
                    return "Boolean";
                case SqlDataType.Date:
                case SqlDataType.DateTime:
                case SqlDataType.DateTime2:
                case SqlDataType.SmallDateTime:
                    return "DateTime";
                case SqlDataType.Decimal:
                case SqlDataType.Money:
                case SqlDataType.Numeric:
                case SqlDataType.SmallMoney:
                    return "Decimal";
                case SqlDataType.Float:
                    return "Double";
                case SqlDataType.Int:
                    return "Int32";
                case SqlDataType.Char:
                case SqlDataType.NChar:
                case SqlDataType.NVarChar:
                case SqlDataType.NVarCharMax:
                case SqlDataType.VarChar:
                case SqlDataType.VarCharMax:
                    return "String";
                case SqlDataType.SmallInt:
                    return "Int16";
                case SqlDataType.TinyInt:
                    return "Byte";
                case SqlDataType.UniqueIdentifier:
                    return "Guid";
                default:
                    throw new ApplicationException("GetCSharpDataType: Unmapped data type = " + sqlDataType.ToString());
            }
        }

        private string GetCSharpDataTypeNullable(
            SqlDataType sqlDataType,
            bool isNullable
        )
        {
            string dataType = GetCSharpDataType(sqlDataType);
            if (!isNullable)
            {
                return dataType;
            }
            switch (dataType)
            {
                case "Boolean":
                case "Byte":
                case "DateTime":
                case "Double":
                case "Guid":
                case "Int16":
                case "Int32":
                    return dataType + "?";
                default:
                    return dataType;
            }
        }

        private string GetConvertToCode(SqlDataType sqlDataType, string valueToConvert, bool isNullable)
        {
            if (isNullable)
            {
                return valueToConvert + " as " + GetCSharpDataTypeNullable(sqlDataType, isNullable);
            }

            return "Convert.To" + GetCSharpDataType(sqlDataType) + "(" + valueToConvert + ")";
        }

        private string GetVariableName(string propertyName)
        {
            return propertyName.First().ToString().ToLower() + propertyName.Substring(1);
        }
    }
}
