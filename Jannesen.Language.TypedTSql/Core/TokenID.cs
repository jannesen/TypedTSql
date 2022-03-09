﻿using System;

namespace Jannesen.Language.TypedTSql.Core
{
    public enum TokenID
    {
        EOF                 = -1,
        InvalidCharacter    = 0,
        WhiteSpace          = 1,
        LineComment,
        BlockComment,
        Name                = 10,
        QuotedName,
        LocalName,
        String,
        Number,
        BinaryValue,
        DataIsland,
        _operators          = 100,
        Equal,
        NotEqual,
        Greater,
        Less,
        Exclamation,
        Dot,
        LrBracket,
        RrBracket,
        Comma,
        Semicolon,
        Colon,
        DoubleColon,
        Divide,
        Module,
        Plus,
        Minus,
        BitNot,
        BitOr,
        BitAnd,
        BitXor,
        LessEqual,
        GreaterEqual,
        PlusAssign,
        MinusAssign,
        MultAssign,
        DivAssign,
        ModAssign,
        AndAssign,
        XorAssign,
        OrAssign,
        _beginkeywords          = 1024,
        ADD,
        ALL,
        ALTER,
        AND,
        ANY,
        APPLY,
        AS,
        ASC,
        ASSEMBLY,
        AUTHORIZATION,
        BACKUP,
        BEGIN,
        BETWEEN,
        BREAK,
        BROWSE,
        BULK,
        BY,
        CASCADE,
        CATCH,
        CHECK,
        CHECKPOINT,
        CLOSE,
        CLUSTERED,
        COALESCE,
        COLLATE,
        COLUMN,
        COMMIT,
        COMPUTE,
        CONSTRAINT,
        CONTAINS,
        CONTAINSTABLE,
        CONTINUE,
        CREATE,
        CROSS,
        CURRENT,
        CURSOR,
        DATABASE,
        DBCC,
        DEALLOCATE,
        DECLARE,
        DEFAULT,
        DELETE,
        DENY,
        DEPENDENTASSEMBLY,
        DESC,
        DISK,
        DISTINCT,
        DISTRIBUTED,
        DOUBLE,
        DROP,
        DUMP,
        ELSE,
        END,
        ERRLVL,
        ESCAPE,
        EXCEPT,
        EXEC,
        EXECUTE,
        EXECUTESQL,
        EXIT,
        EXTERNAL,
        FETCH,
        FILE,
        FILLFACTOR,
        FOR,
        FOREIGN,
        FREETEXT,
        FREETEXTTABLE,
        FROM,
        FULL,
        FUNCTION,
        GOTO,
        GRANT,
        GROUP,
        HANDLERCONFIG,
        HAVING,
        HOLDLOCK,
        IDENTITY_INSERT,
        IDENTITYCOL,
        IF,
        IN,
        INDEX,
        INNER,
        INSERT,
        INTERSECT,
        INTO,
        IS,
        JOIN,
        KEY,
        KILL,
        LIKE,
        LINENO,
        LOAD,
        MERGE,
        METHOD,
        NATIONAL,
        NOCHECK,
        NONCLUSTERED,
        NOT,
        NULL,
        OF,
        OFF,
        OFFSETS,
        ON,
        OPEN,
        OPTION,
        OR,
        ORDER,
        OUTER,
        OVER,
        PERCENT,
        PIVOT,
        PLAN,
        PRECISION,
        PRIMARY,
        PRINT,
        PROC,
        PROCEDURE,
        PROPERTY,
        RAISERROR,
        READ,
        READTEXT,
        RECONFIGURE,
        REFERENCES,
        REPLICATION,
        REQUIRED,
        RESPONSE,
        RESTORE,
        RESTRICT,
        RETURN,
        RETURNS,
        REVERT,
        REVOKE,
        ROLLBACK,
        ROWGUIDCOL,
        RULE,
        SAVE,
        SCHEMA,
        SECURITYAUDIT,
        SELECT,
        SET,
        SETUSER,
        SHUTDOWN,
        SOME,
        SOURCE,
        STATIC,
        STATISTICS,
        TABLE,
        TABLESAMPLE,
        TEXTSIZE,
        THEN,
        THROW,
        TO,
        TOP,
        TRAN,
        TRANSACTION,
        TRIGGER,
        TRUNCATE,
        TRY,
        TSEQUAL,
        TYPE,
        UNION,
        UNIQUE,
        UNPIVOT,
        UPDATETEXT,
        USE,
        VALUES,
        VARYING,
        VIEW,
        WAITFOR,
        WHEN,
        WHERE,
        WHILE,
        WITH,
        WRITETEXT,
        _endkeywords,
        _beginkeywordswithsymbol        = 2048,
        Star,
        CASE,
        CAST,
        CONVERT,
        EXISTS,
        IDENTITY,
        LEFT,
        RIGHT,
        UPDATE,
        _endkeywordswithsymbol
    }
}
