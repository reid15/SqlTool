SQL Tool

Overview:
A SQL Server utility to generate scripts and files.

Requirements:
The program requires the .Net Framework 4.0 or later. 
Windows authentication is used. The user must have permission to view the database objects and data that will be compared.
No SQL Server edition specific features are used. All functions were tested using SQL Server 2017.
The source code references the project in the DatabaseCommon repository.

Repository Contents:
	Bin: The compiled program
	Source: Visual Studio solution with the C# source code
	SQLScripts: SQL Server scripts to set up test data. 
	TestScript.sql: Create tables with test data,.

Program Options:	
	Job Type: Determines which function to execute. The Job Type section below lists the available options.
	Server: The SQL Server instance name.
	Database: The name of the SQL Server database.
	Table: Table name
	Where: For data scripting, a valid SQL WHERE clause to limit the data returned. The WHERE doesn't need to be added to the clause.
	
Job Type:
	CRUD Procs: Generate stored procedures to insert, update, delete and select records from the specified table.
	Script Data: Generate script to insert data into a table. The Where clause can limit the data exported.
	Script Table: Generate script to create a table.
	Script Data And Table: Generate script to create a specified table and to insert the data in that table.
	Data Dictionary: Generate a HTML file with a list of the tables and columns in a specified database to create a data dictionary. Any extended properties for 'MS_Description' for a table or column is listed under Description. For columns, the Name, Data Type, Is Nullable, Primary Key, and Default values are listed.
	Export Database: Each database object is scripted to a file. Separate directories are created for Tables, Stored Procedures, Views, etc. 
	Data Layer: Generate code to create a C# data object from a table definition, and code to call stored procedures to insert, update, deleted and select data from that table. 

Buttons:
	Go: Run the selected Job Type. Any output is displayed in the text box.
	Save: Save any output to a file.
	Copy: Copy the job output from the text box to the clipboard.

Configurations Values:
In the app.config file, there are settings in the <appSettings> section of the file.
defaultServer : Will set a default 'Server' value on the application startup.
defaultDatabase : Will set a default 'Database' value on the application startup.