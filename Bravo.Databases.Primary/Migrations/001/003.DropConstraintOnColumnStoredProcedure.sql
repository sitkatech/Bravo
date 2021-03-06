/****** Object:  StoredProcedure [dbo].[DropConstraintOnColumn]    Script Date: 4/8/2021 10:25:44 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create   procedure [dbo].[DropConstraintOnColumn] @schema nvarchar(100),  @table nvarchar(100), @column nvarchar(100)
as
begin
declare @Command  nvarchar(1000);
select @Command = 'ALTER TABLE ' + @schema + '.' + @table + ' drop constraint ' + d.name
 from sys.tables t
  join    sys.default_constraints d
   on d.parent_object_id = t.object_id
  join    sys.columns c
   on c.object_id = t.object_id
    and c.column_id = d.parent_column_id
 where t.name = @table
  and t.schema_id = schema_id(@schema)
  and c.name = @column
execute (@Command)
end
GO
