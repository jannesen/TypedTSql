DECLARE @cmd        varchar(128);

DECLARE ccmd CURSOR LOCAL STATIC
    FOR SELECT 'KILL '+convert(varchar, s.[session_id])
          FROM (
                    SELECT DISTINCT [request_session_id]
                      FROM master.sys.[dm_tran_locks]
                     WHERE [resource_type]        = 'DATABASE'
                       AND [resource_database_id] = db_id()
               ) l
               INNER JOIN master.sys.[dm_exec_sessions] s ON s.[session_id] = l.[request_session_id]
         WHERE s.[session_id]         <> @@spid
           AND s.[is_user_process]    =  1;

OPEN ccmd;

FETCH ccmd INTO @cmd;
WHILE @@fetch_status=0
BEGIN
    EXECUTE(@cmd);
    FETCH ccmd INTO @cmd;
END

DEALLOCATE ccmd;
