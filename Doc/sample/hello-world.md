# Hello world sample

T-SQL:
```
DROP PROCEDDURE IF EXISTS dbo.hello_world
GO
CREATE PROCEDURE dbo.hello_world
AS
BEGIN
    SELECT 'Hello world'
END
GO
GRANT EXECUTE dbo.hello_world TO public
GO
```

Typed T-SQL
```
PROCEDURE hello_world
GRANT EXECUTE TO public
BEGIN
    SELECT 'Hello world'
END
```