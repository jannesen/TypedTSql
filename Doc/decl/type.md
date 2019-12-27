# TODO

### sample
```
TYPE [t]
FROM INT
WITH TYPECHECK STRICT
VALUES (
	[a] = 1,
	[b] = 2,
	[c] = 3
)

VAR @i = [t]::[b] -- transpiled to DECLARE @i = 2
```