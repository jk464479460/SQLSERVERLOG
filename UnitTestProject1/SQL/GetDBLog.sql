select [RowLog Contents 0] from fn_dblog(null,null)
where AllocUnitName like '%<tableName>%' and Operation in('LOP_INSERT_ROWS', 'LOP_MODIFY_ROW' , 'LOP_DELETE_ROWS')
order by [Current LSN] desc
