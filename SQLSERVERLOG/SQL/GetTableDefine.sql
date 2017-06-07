select obj.name,col.name, column_id, max_length, typ.name
from sys.columns col join sys.objects obj on col.object_id = obj.object_id
join sys.systypes typ on typ.xtype = col.user_type_id
where obj.[type]='U' and obj.name ='<tableName>'
order by column_id