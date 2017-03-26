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
            var options = GetScriptingOptions();

            ScriptObjectType(directoryName, "Tables", database.Tables, options);
            ScriptObjectType(directoryName, "ForeignKeys", database.Tables, GetScriptingOptionsForeignKey());
            ScriptObjectType(directoryName, "Views", database.Views, options);
            ScriptObjectType(directoryName, "StoredProcedures", database.StoredProcedures, options);
            ScriptObjectType(directoryName, "Functions", database.UserDefinedFunctions, options);
            
        }
            
        public static void ScriptObjectType<T>(
            string parentDirectoryName,
            string objectDirectoryName,
            T itemCollection,
            ScriptingOptions options
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
                    ScriptObject(item, filePath, options);
                }
            }
        }

        private static void ScriptObject<T>(T item, string fileName, ScriptingOptions options) where T: IScriptable
        {
            // Instead of ToFileOnly option, check if string has any contents before writing file - Foreign Key script could be easy
            var scriptCollection = item.Script(options);
            var returnSQL = new StringBuilder();
            foreach (var scriptItem in scriptCollection)
            {
                returnSQL.AppendLine(scriptItem);
            }
            string scriptContents = returnSQL.ToString().Trim();
            if (scriptContents.Length > 0)
            {
                File.WriteAllText(fileName, scriptContents);
            }
        }

        private static ScriptingOptions GetScriptingOptions()
        {
            var options = new ScriptingOptions();
            options.DriAll = true;
            options.DriForeignKeys = false;
            options.ExtendedProperties = true;
            options.NoFileGroup = true;
            options.ScriptBatchTerminator = true;
            options.ToFileOnly = false;
            options.Triggers = true;
            return options;
        }

        private static ScriptingOptions GetScriptingOptionsForeignKey()
        {
            var options = new ScriptingOptions();
            options.DriForeignKeys = true;
            options.NoFileGroup = true;
            options.PrimaryObject = false;
            options.ScriptBatchTerminator = true;
            options.ToFileOnly = false;
            return options;
        }
    }
}
