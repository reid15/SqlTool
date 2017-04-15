﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlTool
{
    public partial class SqlToolForm : Form
    {
        public SqlToolForm()
        {
            InitializeComponent();
            cboJobType.DataSource = JobType.GetJobTypeList();

            txtServer.Text = ConfigurationManager.AppSettings["defaultServer"];
            txtDatabase.Text = ConfigurationManager.AppSettings["defaultDatabase"];
        }
        
        private void txtDatabase_Leave(object sender, EventArgs e)
        {
            if (cboTable.Enabled)
            {
                cboTable.DataSource = SqlSchema.GetTableList(txtServer.Text, txtDatabase.Text);
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            try
            {
                var jobSelection = (JobTypeEntry)cboJobType.SelectedValue;
                var tableInfo = (TableInfo)cboTable.SelectedValue;

                switch (jobSelection.JobTypeId)
                {
                    case JobTypeEnum.Crud:
                        txtResults.Text = CodeGeneration.GetCrudProcs(txtServer.Text, txtDatabase.Text, tableInfo);
                        break;
                    case JobTypeEnum.ScriptData:
                        txtResults.Text = ScriptData.GetScriptedData(txtServer.Text, txtDatabase.Text, tableInfo, txtWhere.Text);
                        break;
                    case JobTypeEnum.ScriptTable:
                        txtResults.Text = ScriptSchema.GetTableScript(txtServer.Text, txtDatabase.Text, tableInfo.SchemaName, tableInfo.TableName);
                        break;
                    case JobTypeEnum.ScriptDataAndTable:
                        var scriptString = ScriptSchema.GetTableScript(txtServer.Text, txtDatabase.Text, tableInfo.SchemaName, tableInfo.TableName);
                        scriptString += Environment.NewLine;
                        scriptString += ScriptData.GetScriptedData(txtServer.Text, txtDatabase.Text, tableInfo, "");
                        txtResults.Text = scriptString;
                        break;
                    case JobTypeEnum.DataDictionary:
                        txtResults.Text = DataDictionary.GetDataDictionary(txtServer.Text, txtDatabase.Text);
                        break;
                    case JobTypeEnum.ExportDatabase:
                        string exportDirectory = SelectDirectory();
                        if (exportDirectory.Length > 0)
                        {
                            txtResults.Text = "Exporting...";
                            Cursor.Current = Cursors.WaitCursor;
                            Application.DoEvents();
                            ScriptSchema.ScriptDatabase(txtServer.Text, txtDatabase.Text, exportDirectory);
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("Export Completed");
                            txtResults.Text = "";
                        }
                        break;
                    default:
                        break;
                }
            } catch (Exception exception)
            {
                ExceptionHandler(exception);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var jobSelection = (JobTypeEntry)cboJobType.SelectedValue;
            if (txtResults.Text.Length > 0 && jobSelection.SaveFile)
            {
                var fileSaver = new SaveFileDialog();
                fileSaver.Filter = jobSelection.FileTypeFilter;
                fileSaver.FileName = jobSelection.FileName;
                fileSaver.Title = "Save File";
                if (fileSaver.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(fileSaver.FileName, txtResults.Text);
                }
            }
        }

        private void cboJobType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var jobSelection = (JobTypeEntry)cboJobType.SelectedValue;
            cboTable.Enabled = !(jobSelection.JobTypeId == JobTypeEnum.DataDictionary || jobSelection.JobTypeId == JobTypeEnum.ExportDatabase);
            txtWhere.Enabled = (jobSelection.JobTypeId == JobTypeEnum.ScriptData);
        }

        private void ExceptionHandler(
            Exception exception
        )
        {
            string exceptionText = exception.Message;
            string caption = "Exception";
            MessageBoxButtons button = MessageBoxButtons.OK;
            MessageBox.Show(exceptionText, caption, button);
        }

        private string SelectDirectory()
        {
            var fbd = new FolderBrowserDialog();
            if(fbd.ShowDialog() == DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            return "";
        }
    }
}