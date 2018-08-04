using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlTool
{
    public class DataDictionary
    {
        public static string GetDataDictionary(
            string serverName,
            string databaseName
        )
        {
            var database = DatabaseCommon.DatabaseSchema.GetDatabase(serverName, databaseName);
            var returnString = new StringBuilder();
            returnString.AppendLine("<html>");
            returnString.AppendLine("<head>");
            returnString.Append("<title>");
            returnString.Append("Data Dictionary");
            returnString.Append("</title>");
            returnString.AppendLine("</head>");
            returnString.AppendLine("<body>");
            returnString.AppendLine("Data Dictionary<br>");
            returnString.AppendLine("Database: " + databaseName + "<br>");
            returnString.AppendLine(DateTime.Now.ToString("MMMM d, yyyy") + "<br>");
            returnString.AppendLine("<br>");

            var tableList = new StringBuilder();
            var bodyString = new StringBuilder();

            foreach (Table table in database.Tables)
            {
                var fullTableName = table.Schema + "." + table.Name;
                tableList.AppendLine("<a href='#" + fullTableName + "'>" + fullTableName + "</a><br>");
                bodyString.AppendLine("<h3 id='" + fullTableName + "'>Table:" + fullTableName + "</h3><br>");
                var tableDescription = table.ExtendedProperties["MS_Description"];
                if (tableDescription != null)
                {
                    bodyString.AppendLine("Table Description:" + tableDescription.Value.ToString() + "<br>");
                }
                bodyString.AppendLine("<Table>");
                bodyString.AppendLine("<TH>Name</TH>");
                bodyString.AppendLine("<TH>Data Type</TH>");
                bodyString.AppendLine("<TH>Is Nullable</TH>");
                bodyString.AppendLine("<TH>Primary Key</TH>");
                bodyString.AppendLine("<TH>Default</TH>");
                bodyString.AppendLine("<TH>Description</TH>");
                foreach (Column column in table.Columns)
                {
                    bodyString.AppendLine("<TR>");
                    bodyString.AppendLine("<TD>" + column.Name + "</TD>");
                    bodyString.AppendLine("<TD>" + DatabaseCommon.DatabaseSchema.GetColumnDataType(column) + "</TD>");
                    bodyString.AppendLine("<TD>" + column.Nullable.ToString() + "</TD>");
                    bodyString.AppendLine("<TD>" + column.InPrimaryKey.ToString() + "</TD>");
                    string defaultValue = "";
                    if (column.Identity)
                    {
                        defaultValue = "Identity";
                    } else 
                    {
                        defaultValue = SqlSchema.GetColumnDefaultValue(column);
                    }
                    bodyString.AppendLine("<TD>" + defaultValue + "</TD>");
                    var columnDescription = column.ExtendedProperties["MS_Description"];
                    if (columnDescription != null)
                    {
                        bodyString.AppendLine("<TD>" + columnDescription.Value.ToString() + "</TD>");
                    }
                    returnString.AppendLine("</TR>");
                }
                bodyString.AppendLine("</Table>");
                bodyString.AppendLine("<br>");
            }
            returnString.AppendLine(tableList.ToString());
            returnString.AppendLine("<br>");
            returnString.AppendLine(bodyString.ToString());
            returnString.AppendLine();

            returnString.AppendLine("</body>");
            returnString.AppendLine("</html>");
            return returnString.ToString();
        }
    }
}
