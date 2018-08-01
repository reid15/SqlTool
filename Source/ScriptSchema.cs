using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

            ScriptObjectType(directoryName, "Schemas", database.Schemas.Cast<Schema>().ToList(), options);
            ScriptObjectType(directoryName, "Tables", database.Tables.Cast<Table>().ToList(), options);
            ScriptObjectType(directoryName, "ForeignKeysTriggers", GetTablesWithForeignKeysOrTriggers(database), GetScriptingOptionsForeignKeyTrigger());
            ScriptObjectType(directoryName, "Views", database.Views.Cast<View>().ToList(), options);
            ScriptObjectType(directoryName, "StoredProcedures", database.StoredProcedures.Cast<StoredProcedure>().ToList(), options);
            ScriptObjectType(directoryName, "Functions", database.UserDefinedFunctions.Cast<UserDefinedFunction>().ToList(), options);
            ScriptObjectType(directoryName, "Sequences", database.Sequences.Cast<Sequence>().ToList(), options);
            ScriptObjectType(directoryName, "UserDefinedTableTypes", database.UserDefinedTableTypes.Cast<UserDefinedTableType>().ToList(), options);

            ScriptReferenceData(database, directoryName, "ReferenceData");
        }
            
        public static void ScriptObjectType<T>(
            string parentDirectoryName,
            string objectDirectoryName,
            List<T> itemCollection,
            ScriptingOptions options
        ) where T: ScriptNameObjectBase 
        {
            if (itemCollection.Count == 0)
            {
                return;
            }
            string objectTypeDirectory = string.Empty;
            foreach (IScriptable item in itemCollection)
            {
                var smoItem = (ScriptNameObjectBase)item;
                //if (!(bool)smoItem.Properties["IsSystemObject"].Value)
                if (!IsSystemObject(smoItem))
                {
                    if (objectTypeDirectory == string.Empty)
                    {
                        objectTypeDirectory = Common.CreateDirectory(parentDirectoryName, objectDirectoryName);
                    }
                    
                    string scriptName = smoItem.Name + ".sql";
                    if (smoItem.GetType().Name != "Schema")
                    {
                        var schemaObject = (ScriptSchemaObjectBase)smoItem;
                        scriptName = schemaObject.Schema + "." + scriptName;
                    }
                    
                    string filePath = Path.Combine(objectTypeDirectory, scriptName);
                    options.FileName = filePath;
                    item.Script(options);
                }
            }
        }

        private static bool IsSystemObject(ScriptNameObjectBase smoItem)
        {
            if (!smoItem.Properties.Contains("IsSystemObject"))
            {
                return false;
            }
            return (bool)smoItem.Properties["IsSystemObject"].Value;
        }

        private static ScriptingOptions GetScriptingOptions()
        {
            var options = new ScriptingOptions();
            options.DriAll = true;
            options.DriForeignKeys = false;
            options.ExtendedProperties = true;
            options.NoFileGroup = true;
            options.ScriptBatchTerminator = true;
            options.ToFileOnly = true;
            options.Triggers = false;
            return options;
        }

        private static ScriptingOptions GetScriptingOptionsForeignKeyTrigger()
        {
            var options = new ScriptingOptions();
            options.DriForeignKeys = true;
            options.NoFileGroup = true;
            options.PrimaryObject = false;
            options.ScriptBatchTerminator = true;
            options.ToFileOnly = true;
            options.Triggers = true;
            return options;
        }

        private static ScriptingOptions GetScriptingOptionsData()
        {
            var options = new ScriptingOptions();
            options.ScriptSchema = false;
            options.ScriptData = true;
            options.ScriptDrops = false;
            options.ToFileOnly = true;
            return options;
        }

        private static List<Table> GetTablesWithForeignKeysOrTriggers(Database database)
        {
            List<Table> returnList = new List<Table>();
            foreach (Table table in database.Tables)
            {
                if (!table.IsSystemObject && (table.ForeignKeys.Count > 0 || table.Triggers.Count > 0))
                {
                    returnList.Add(table);
                }
            }
            return returnList;
        }

        private static void ScriptReferenceData(Database database, string parentDirectoryName, string objectDirectoryName)
        {
            string objectTypeDirectory = string.Empty;
            var options = GetScriptingOptionsData();
            options.ToFileOnly = true;

            foreach (Table table in database.Tables)
            {
                if (!table.IsSystemObject && table.ExtendedProperties["HasReferenceData"] != null &&
                    table.ExtendedProperties["HasReferenceData"].Value.ToString() == "true")
                {
                    if (objectTypeDirectory == string.Empty)
                    {
                        objectTypeDirectory = Common.CreateDirectory(parentDirectoryName, objectDirectoryName);
                    }
                    string scriptName = table.Schema + "." + table.Name + ".sql";
                    string filePath = Path.Combine(objectTypeDirectory, scriptName);
                    options.FileName = filePath;
                    table.EnumScript(options);
                }
            }
        }
    }
}
