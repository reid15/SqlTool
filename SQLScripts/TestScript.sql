
drop table if exists dbo.TestTable;

create table dbo.TestTable(
RecordId int not null primary key,
UpdatedAt datetime not null,
DisplayName varchar(50) not null,
SortOrder tinyint not null,
Price decimal(6,2) not null
);

insert into dbo.TestTable(RecordId, UpdatedAt, DisplayName, SortOrder, Price)
values (1, getdate(), 'Record 1', 1, 20.00);

insert into dbo.TestTable(RecordId, UpdatedAt, DisplayName, SortOrder, Price)
values (2, getdate(), 'Record 2', 2, 200.00);

insert into dbo.TestTable(RecordId, UpdatedAt, DisplayName, SortOrder, Price)
values (3, getdate(), 'Record 3', 3, 2000.00);

go

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Numeric ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TestTable', @level2type=N'COLUMN',@level2name=N'RecordId'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date and time of the last update for this record' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TestTable', @level2type=N'COLUMN',@level2name=N'UpdatedAt'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name to display to the user' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TestTable', @level2type=N'COLUMN',@level2name=N'DisplayName'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Records should appear in sequential order' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TestTable', @level2type=N'COLUMN',@level2name=N'SortOrder'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Cost of the item' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TestTable', @level2type=N'COLUMN',@level2name=N'Price'
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Table for testing' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TestTable'
GO





