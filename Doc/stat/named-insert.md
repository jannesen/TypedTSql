# Named insert

SQL insert is prone to errors. This because the order of the fields between target-column and select is important.<br/>
TTSQL implements named target column insert.


## syntax
```SQL
INSERT INTO <target> (*)
SELECT [target-colum] = <expr>,...
```



