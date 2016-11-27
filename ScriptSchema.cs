using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlTool
{
    public class ScriptSchema
    {
        public static string GetTableScript(
            string serverName,
            string databaseName,
            string schemaName,
            string tableName
        )
        {
            var database = SqlSchema.GetDatabase(serverName, databaseName);
            return GetTableScript(database, schemaName, tableName);
        }

        public static string GetTableScript(
            Database database,
            string schemaName,
            string tableName
        )
        {
            var options = new ScriptingOptions();
            options.DriAll = true;
            options.NoFileGroup = true;
            var table = SqlSchema.GetTable(database, schemaName, tableName);
            var scriptCollection = table.Script(options);
            var returnSQL = new StringBuilder();
            foreach (var item in scriptCollection)
            {
                returnSQL.AppendLine(item);
            }
            return returnSQL.ToString();
        }

        public static void ScriptDatabase(
            string serverName,
            string databaseName,
            string directoryName
        )
        {
            if (Directory.Exists(directoryName))
            {
                Directory.Delete(directoryName, true);
            }
            Directory.CreateDirectory(directoryName);

            var database = SqlSchema.GetDatabase(serverName, databaseName);

            ScriptObjectType(directoryName, "Tables", database.Tables);
            ScriptObjectType(directoryName, "Views", database.Views);
            ScriptObjectType(directoryName, "StoredProcedures", database.StoredProcedures);
            ScriptObjectType(directoryName, "Functions", database.UserDefinedFunctions);
        }
            
        public static void ScriptObjectType<T>(
            string parentDirectoryName,
            string objectDirectoryName,
            T itemCollection
        ) where T: SchemaCollectionBase
        {
            if (itemCollection.Count == 0)
            {
                return;
            }
            string objectTypeDirectory = Path.Combine(parentDirectoryName, objectDirectoryName);
            Directory.CreateDirectory(objectTypeDirectory);
            foreach(IScriptable item in itemCollection) 
            {
                var smoItem = (ScriptSchemaObjectBase)item;
                if (!(bool)smoItem.Properties["IsSystemObject"].Value)
                {
                    string scriptName = smoItem.Schema + "." + smoItem.Name + ".sql";
                    string filePath = Path.Combine(objectTypeDirectory, scriptName);
                    ScriptObject(item, filePath);
                }
            }
        }

        private static string ScriptObject<T>(T item, string fileName) where T: IScriptable
        {
            var options = new ScriptingOptions();
            options.DriAll = true;
            options.ExtendedProperties = true;
            options.FileName = fileName;
            options.NoFileGroup = true;
            options.ScriptBatchTerminator = true;
            options.ToFileOnly = true;
            options.Triggers = true;
            var scriptCollection = item.Script(options);
            var returnSQL = new StringBuilder();
            foreach (var scriptItem in scriptCollection)
            {
                returnSQL.AppendLine(scriptItem);
            }
            return returnSQL.ToString();
        }
    }
}
