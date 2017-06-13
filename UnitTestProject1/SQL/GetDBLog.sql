SELECT allocunitname,operation,[RowLog Contents 0] as r0,[RowLog Contents 1]as r1 
from::fn_dblog (null, null)   
where allocunitname like '%<tableName>%'and operation in('LOP_INSERT_ROWS','LOP_DELETE_ROWS'/*, 'LOP_MODIFY_ROW'*/)
order by [Current LSN] desc

