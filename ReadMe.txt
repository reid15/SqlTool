SQL Tool

A SQL Server utility to generate scripts and files.

Job Type:
CRUD Procs - Generate stored procedures to insert, update, delete and select records from the specified table.
Script Data - Generate script to insert data into a table. The Where clause can limit the data exported.
Script Table - Generate script to create a table.
Script Data And Table - Generate script to create a specified table and to insert the data in that table.
Data Dictionary - Generate a HTML file with a list of the tables and columns in a specified database to create a data dictionary. Any extended properties for 'MS_Description' for a table or column is listed under Description. For columns, the Name, Data Type, Is Nullable, Primary Key, and Default values are listed.
Export Database - Each database object is scripted to a file. Separate directories are created for Tables, Stored Procedures, Views, etc. 
Data Layer - Generate code to create a C# data object from a table definition, and code to call stored procedures to insert, update, deleted and select data from that table. 

Server: The SQL Server instance name.
Database: The name of the SQL Server database.
Table: Table name
Where: For data scripting, a valid SQL WHERE clause to limit the data returned. The WHERE doesn't need to be added to the clause.

Go: Run the selected Job Type. Any output is displayed in the text box.
Save: Save any output to a file.
Copy: Copy the job output from the text box to the clipboard.

Configurations Values:
In the app.config file, there are settings in the <appSettings> section of the file.
defaultServer : Will set a default 'Server' value on the application startup.
defaultDatabase : Will set a default 'Database' value on the application startup.