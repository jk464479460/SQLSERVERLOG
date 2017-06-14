SELECT [Page ID] PageId,allocunitname,operation, [Slot ID] slotId,[Offset in Row] Offset, [Modify Size] ModifySize,[RowLog Contents 0] as r0,[RowLog Contents 1]as r1, [RowLog Contents 2] r2,
[RowLog Contents 3] r3, [RowLog Contents 4] r4
from::fn_dblog (null, null)   
where allocunitname like '%<tableName>%'and operation in(/*'LOP_INSERT_ROWS','LOP_DELETE_ROWS',*/ 'LOP_MODIFY_ROW')
order by [Current LSN] desc

