using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlTool
{
    public enum JobTypeEnum
    {
        Crud,
        ScriptData,
        ScriptTable,
        ScriptDataAndTable,
        DataDictionary,
        ExportDatabase,
        DataLayer
    }

    public class JobTypeEntry
    {
        public JobTypeEnum JobTypeId { get; set; }
        public string JobTypeName { get; set; }
        public string FileTypeFilter { get; set; }
        public string FileName { get; set; }
        public bool SaveFile { get; set; }

        public JobTypeEntry(
            JobTypeEnum jobTypeId,
            string jobTypeName,
            bool saveFile,
            string fileTypeFilter,
            string fileName
        )
        {
            JobTypeId = jobTypeId;
            JobTypeName = jobTypeName;
            SaveFile = saveFile;
            FileTypeFilter = fileTypeFilter;
            FileName = fileName;
        }
        public override string ToString()
        {
            return JobTypeName;
        }
    }

    public class JobType
    {
        public static List<JobTypeEntry> GetJobTypeList()
        {
            var returnList = new List<JobTypeEntry>();
            returnList.Add(new JobTypeEntry(JobTypeEnum.Crud, "CRUD Procs", true, "SQL Script | *.sql", "CRUDProcs"));
            returnList.Add(new JobTypeEntry(JobTypeEnum.ScriptData, "Script Data", true, "SQL Script | *.sql", "DataScript"));
            returnList.Add(new JobTypeEntry(JobTypeEnum.ScriptTable, "Script Table", true, "SQL Script | *.sql", "TableScript"));
            returnList.Add(new JobTypeEntry(JobTypeEnum.ScriptDataAndTable, "Script Data And Table", true, "SQL Script | *.sql", "TableAndDataScript"));
            returnList.Add(new JobTypeEntry(JobTypeEnum.DataDictionary, "Data Dictionary", true, "HTML|*.html", "DataDictionary"));
            returnList.Add(new JobTypeEntry(JobTypeEnum.ExportDatabase, "ExportDatabase", false, "", ""));
            returnList.Add(new JobTypeEntry(JobTypeEnum.DataLayer, "Data Layer", true, "C#|*.cs", "DataLayer"));

            return returnList;
        }
 
    }
}
