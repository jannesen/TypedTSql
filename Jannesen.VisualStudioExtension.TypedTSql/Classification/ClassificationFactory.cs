﻿using System;
using Microsoft.VisualStudio.Text.Classification;
using LTTS = Jannesen.Language.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.Classification
{
    internal class ClassificationFactory
    {
        public      readonly    IClassificationType                             cfComment;
        public      readonly    IClassificationType                             cfName;
        public      readonly    IClassificationType                             cfNumber;
        public      readonly    IClassificationType                             cfString;
        public      readonly    IClassificationType                             cfOperator;
        public      readonly    IClassificationType                             cfKeyword;
        public      readonly    IClassificationType                             cfLocalVariable;
        public      readonly    IClassificationType                             cfGlobalVariable;
        public      readonly    IClassificationType                             cfBuildIn;
        public      readonly    IClassificationType                             cfType;
        public      readonly    IClassificationType                             cfTable;
        public      readonly    IClassificationType                             cfView;
        public      readonly    IClassificationType                             cfFunction;
        public      readonly    IClassificationType                             cfStoredProcedure;
        public      readonly    IClassificationType                             cfParameter;
        public      readonly    IClassificationType                             cfColumn;
        public      readonly    IClassificationType                             cfUDTValue;

        public                                                                  ClassificationFactory(IClassificationTypeRegistryService registry)
        {
            cfComment         = registry.GetClassificationType(ClassificationTypes.Comment);
            cfName            = registry.GetClassificationType(ClassificationTypes.Name);
            cfNumber          = registry.GetClassificationType(ClassificationTypes.Number);
            cfString          = registry.GetClassificationType(ClassificationTypes.String);
            cfOperator        = registry.GetClassificationType(ClassificationTypes.Operator);
            cfKeyword         = registry.GetClassificationType(ClassificationTypes.Keyword);
            cfLocalVariable   = registry.GetClassificationType(ClassificationTypes.LocalVariable);
            cfGlobalVariable  = registry.GetClassificationType(ClassificationTypes.GlobalVariable);
            cfBuildIn         = registry.GetClassificationType(ClassificationTypes.BuildIn);
            cfType            = registry.GetClassificationType(ClassificationTypes.Type);
            cfTable           = registry.GetClassificationType(ClassificationTypes.Table);
            cfView            = registry.GetClassificationType(ClassificationTypes.View);
            cfFunction        = registry.GetClassificationType(ClassificationTypes.Function);
            cfStoredProcedure = registry.GetClassificationType(ClassificationTypes.StoredProcedure);
            cfParameter       = registry.GetClassificationType(ClassificationTypes.Parameter);
            cfColumn          = registry.GetClassificationType(ClassificationTypes.Column);
            cfUDTValue        = registry.GetClassificationType(ClassificationTypes.UDTValue);
        }

        public                  IClassificationType                             TokenClassificationType(LTTS.Core.Token token)
        {
            var symbol = (token as LTTS.Core.TokenWithSymbol)?.SymbolData?.GetClassificationSymbol();

            if (symbol != null) {
                switch(symbol.Type) {
                case LTTS.DataModel.SymbolType.BuildinFunction:                        return cfBuildIn;
                case LTTS.DataModel.SymbolType.TypeUser:                               return cfType;
                case LTTS.DataModel.SymbolType.TypeExternal:                           return cfType;
                case LTTS.DataModel.SymbolType.TypeTable:                              return cfType;
                case LTTS.DataModel.SymbolType.TableInternal:                          return cfTable;
                case LTTS.DataModel.SymbolType.TableSystem:                            return cfTable;
                case LTTS.DataModel.SymbolType.TableUser:                              return cfTable;
                case LTTS.DataModel.SymbolType.View:                                   return cfView;
                case LTTS.DataModel.SymbolType.Function:                               return cfFunction;
                case LTTS.DataModel.SymbolType.FunctionScalar:                         return cfFunction;
                case LTTS.DataModel.SymbolType.FunctionScalar_clr:                     return cfFunction;
                case LTTS.DataModel.SymbolType.FunctionInlineTable:                    return cfFunction;
                case LTTS.DataModel.SymbolType.FunctionMultistatementTable:            return cfFunction;
                case LTTS.DataModel.SymbolType.FunctionMultistatementTable_clr:        return cfFunction;
                case LTTS.DataModel.SymbolType.FunctionAggregateFunction_clr:          return cfFunction;
                case LTTS.DataModel.SymbolType.StoredProcedure:                        return cfStoredProcedure;
                case LTTS.DataModel.SymbolType.StoredProcedure_clr:                    return cfStoredProcedure;
                case LTTS.DataModel.SymbolType.StoredProcedure_extended:               return cfStoredProcedure;
                case LTTS.DataModel.SymbolType.Parameter:                              return cfParameter;
                case LTTS.DataModel.SymbolType.Column:                                 return cfColumn;
                case LTTS.DataModel.SymbolType.UDTValue:                               return cfUDTValue;
                }
            }

            switch(token.ID) {
            case LTTS.Core.TokenID.BlockComment:                    return cfComment;
            case LTTS.Core.TokenID.LineComment:                     return cfComment;
            case LTTS.Core.TokenID.Name:                            return token.isKeyword ? cfKeyword : cfName;
            case LTTS.Core.TokenID.QuotedName:                      return cfName;
            case LTTS.Core.TokenID.LocalName:                       return (token.Text.StartsWith("@@", StringComparison.Ordinal) ? cfGlobalVariable : cfLocalVariable);
            case LTTS.Core.TokenID.String:                          return cfString;
            case LTTS.Core.TokenID.Number:                          return cfNumber;
            case LTTS.Core.TokenID.BinaryValue:                     return cfNumber;

            case LTTS.Core.TokenID.Equal:                           return cfOperator;
            case LTTS.Core.TokenID.NotEqual:                        return cfOperator;
            case LTTS.Core.TokenID.Greater:                         return cfOperator;
            case LTTS.Core.TokenID.Less:                            return cfOperator;

            case LTTS.Core.TokenID.Star:                            return cfOperator;
            case LTTS.Core.TokenID.Divide:                          return cfOperator;
            case LTTS.Core.TokenID.Module:                          return cfOperator;
            case LTTS.Core.TokenID.Plus:                            return cfOperator;
            case LTTS.Core.TokenID.Minus:                           return cfOperator;
            case LTTS.Core.TokenID.BitNot:                          return cfOperator;
            case LTTS.Core.TokenID.BitOr:                           return cfOperator;
            case LTTS.Core.TokenID.BitAnd:                          return cfOperator;
            case LTTS.Core.TokenID.BitXor:                          return cfOperator;
            case LTTS.Core.TokenID.LessEqual:                       return cfOperator;
            case LTTS.Core.TokenID.GreaterEqual:                    return cfOperator;
            case LTTS.Core.TokenID.PlusAssign:                      return cfOperator;
            case LTTS.Core.TokenID.MinusAssign:                     return cfOperator;
            case LTTS.Core.TokenID.MultAssign:                      return cfOperator;
            case LTTS.Core.TokenID.DivAssign:                       return cfOperator;
            case LTTS.Core.TokenID.ModAssign:                       return cfOperator;
            case LTTS.Core.TokenID.AndAssign:                       return cfOperator;
            case LTTS.Core.TokenID.XorAssign:                       return cfOperator;
            case LTTS.Core.TokenID.OrAssign:                        return cfOperator;

            case LTTS.Core.TokenID.AND:                             return cfOperator;
            case LTTS.Core.TokenID.BETWEEN:                         return cfOperator;
            case LTTS.Core.TokenID.IN:                              return cfOperator;
            case LTTS.Core.TokenID.EXISTS:                          return cfOperator;
            case LTTS.Core.TokenID.NOT:                             return cfOperator;
            case LTTS.Core.TokenID.NULL:                            return cfOperator;
            case LTTS.Core.TokenID.OR:                              return cfOperator;

            case LTTS.Core.TokenID.ADD:                             return cfKeyword;
            case LTTS.Core.TokenID.ALL:                             return cfKeyword;
            case LTTS.Core.TokenID.ALTER:                           return cfKeyword;
            case LTTS.Core.TokenID.ANY:                             return cfKeyword;
            case LTTS.Core.TokenID.APPLY:                           return cfKeyword;
            case LTTS.Core.TokenID.AS:                              return cfKeyword;
            case LTTS.Core.TokenID.ASC:                             return cfKeyword;
            case LTTS.Core.TokenID.ASSEMBLY:                        return cfKeyword;
            case LTTS.Core.TokenID.AUTHORIZATION:                   return cfKeyword;
            case LTTS.Core.TokenID.BACKUP:                          return cfKeyword;
            case LTTS.Core.TokenID.BEGIN:                           return cfKeyword;
            case LTTS.Core.TokenID.BREAK:                           return cfKeyword;
            case LTTS.Core.TokenID.BROWSE:                          return cfKeyword;
            case LTTS.Core.TokenID.BULK:                            return cfKeyword;
            case LTTS.Core.TokenID.BY:                              return cfKeyword;
            case LTTS.Core.TokenID.CASCADE:                         return cfKeyword;
            case LTTS.Core.TokenID.CASE:                            return cfKeyword;
            case LTTS.Core.TokenID.CAST:                            return cfKeyword;
            case LTTS.Core.TokenID.CATCH:                           return cfKeyword;
            case LTTS.Core.TokenID.CHECK:                           return cfKeyword;
            case LTTS.Core.TokenID.CHECKPOINT:                      return cfKeyword;
            case LTTS.Core.TokenID.CLOSE:                           return cfKeyword;
            case LTTS.Core.TokenID.CLUSTERED:                       return cfKeyword;
            case LTTS.Core.TokenID.COALESCE:                        return cfKeyword;
            case LTTS.Core.TokenID.COLLATE:                         return cfKeyword;
            case LTTS.Core.TokenID.COLUMN:                          return cfKeyword;
            case LTTS.Core.TokenID.COMMIT:                          return cfKeyword;
            case LTTS.Core.TokenID.COMPUTE:                         return cfKeyword;
            case LTTS.Core.TokenID.CONSTRAINT:                      return cfKeyword;
            case LTTS.Core.TokenID.CONTAINS:                        return cfKeyword;
            case LTTS.Core.TokenID.CONTAINSTABLE:                   return cfKeyword;
            case LTTS.Core.TokenID.CONTINUE:                        return cfKeyword;
            case LTTS.Core.TokenID.CONVERT:                         return cfKeyword;
            case LTTS.Core.TokenID.CREATE:                          return cfKeyword;
            case LTTS.Core.TokenID.CROSS:                           return cfKeyword;
            case LTTS.Core.TokenID.CURRENT:                         return cfKeyword;
            case LTTS.Core.TokenID.CURSOR:                          return cfKeyword;
            case LTTS.Core.TokenID.DATABASE:                        return cfKeyword;
            case LTTS.Core.TokenID.DBCC:                            return cfKeyword;
            case LTTS.Core.TokenID.DEALLOCATE:                      return cfKeyword;
            case LTTS.Core.TokenID.DECLARE:                         return cfKeyword;
            case LTTS.Core.TokenID.DEFAULT:                         return cfKeyword;
            case LTTS.Core.TokenID.DELETE:                          return cfKeyword;
            case LTTS.Core.TokenID.DENY:                            return cfKeyword;
            case LTTS.Core.TokenID.DEPENDENTASSEMBLY:               return cfKeyword;
            case LTTS.Core.TokenID.DESC:                            return cfKeyword;
            case LTTS.Core.TokenID.DISK:                            return cfKeyword;
            case LTTS.Core.TokenID.DISTINCT:                        return cfKeyword;
            case LTTS.Core.TokenID.DISTRIBUTED:                     return cfKeyword;
            case LTTS.Core.TokenID.DOUBLE:                          return cfKeyword;
            case LTTS.Core.TokenID.DROP:                            return cfKeyword;
            case LTTS.Core.TokenID.DUMP:                            return cfKeyword;
            case LTTS.Core.TokenID.ELSE:                            return cfKeyword;
            case LTTS.Core.TokenID.END:                             return cfKeyword;
            case LTTS.Core.TokenID.ERRLVL:                          return cfKeyword;
            case LTTS.Core.TokenID.ESCAPE:                          return cfKeyword;
            case LTTS.Core.TokenID.EXCEPT:                          return cfKeyword;
            case LTTS.Core.TokenID.EXEC:                            return cfKeyword;
            case LTTS.Core.TokenID.EXECUTE:                         return cfKeyword;
            case LTTS.Core.TokenID.EXIT:                            return cfKeyword;
            case LTTS.Core.TokenID.EXTERNAL:                        return cfKeyword;
            case LTTS.Core.TokenID.FETCH:                           return cfKeyword;
            case LTTS.Core.TokenID.FILE:                            return cfKeyword;
            case LTTS.Core.TokenID.FILLFACTOR:                      return cfKeyword;
            case LTTS.Core.TokenID.FOR:                             return cfKeyword;
            case LTTS.Core.TokenID.FOREIGN:                         return cfKeyword;
            case LTTS.Core.TokenID.FREETEXT:                        return cfKeyword;
            case LTTS.Core.TokenID.FREETEXTTABLE:                   return cfKeyword;
            case LTTS.Core.TokenID.FROM:                            return cfKeyword;
            case LTTS.Core.TokenID.FULL:                            return cfKeyword;
            case LTTS.Core.TokenID.FUNCTION:                        return cfKeyword;
            case LTTS.Core.TokenID.GOTO:                            return cfKeyword;
            case LTTS.Core.TokenID.GRANT:                           return cfKeyword;
            case LTTS.Core.TokenID.GROUP:                           return cfKeyword;
            case LTTS.Core.TokenID.HANDLERCONFIG:                   return cfKeyword;
            case LTTS.Core.TokenID.HAVING:                          return cfKeyword;
            case LTTS.Core.TokenID.HOLDLOCK:                        return cfKeyword;
            case LTTS.Core.TokenID.IDENTITY:                        return cfKeyword;
            case LTTS.Core.TokenID.IDENTITY_INSERT:                 return cfKeyword;
            case LTTS.Core.TokenID.IDENTITYCOL:                     return cfKeyword;
            case LTTS.Core.TokenID.IF:                              return cfKeyword;
            case LTTS.Core.TokenID.INDEX:                           return cfKeyword;
            case LTTS.Core.TokenID.INNER:                           return cfKeyword;
            case LTTS.Core.TokenID.INSERT:                          return cfKeyword;
            case LTTS.Core.TokenID.INTERSECT:                       return cfKeyword;
            case LTTS.Core.TokenID.INTO:                            return cfKeyword;
            case LTTS.Core.TokenID.IS:                              return cfKeyword;
            case LTTS.Core.TokenID.JOIN:                            return cfKeyword;
            case LTTS.Core.TokenID.KEY:                             return cfKeyword;
            case LTTS.Core.TokenID.KILL:                            return cfKeyword;
            case LTTS.Core.TokenID.LEFT:                            return cfKeyword;
            case LTTS.Core.TokenID.LIKE:                            return cfKeyword;
            case LTTS.Core.TokenID.LINENO:                          return cfKeyword;
            case LTTS.Core.TokenID.LOAD:                            return cfKeyword;
            case LTTS.Core.TokenID.MERGE:                           return cfKeyword;
            case LTTS.Core.TokenID.METHOD:                          return cfKeyword;
            case LTTS.Core.TokenID.NATIONAL:                        return cfKeyword;
            case LTTS.Core.TokenID.NOCHECK:                         return cfKeyword;
            case LTTS.Core.TokenID.NONCLUSTERED:                    return cfKeyword;
            case LTTS.Core.TokenID.OF:                              return cfKeyword;
            case LTTS.Core.TokenID.OFF:                             return cfKeyword;
            case LTTS.Core.TokenID.OFFSETS:                         return cfKeyword;
            case LTTS.Core.TokenID.ON:                              return cfKeyword;
            case LTTS.Core.TokenID.OPEN:                            return cfKeyword;
            case LTTS.Core.TokenID.OPTION:                          return cfKeyword;
            case LTTS.Core.TokenID.ORDER:                           return cfKeyword;
            case LTTS.Core.TokenID.OUTER:                           return cfKeyword;
            case LTTS.Core.TokenID.OVER:                            return cfKeyword;
            case LTTS.Core.TokenID.PERCENT:                         return cfKeyword;
            case LTTS.Core.TokenID.PIVOT:                           return cfKeyword;
            case LTTS.Core.TokenID.PLAN:                            return cfKeyword;
            case LTTS.Core.TokenID.PRECISION:                       return cfKeyword;
            case LTTS.Core.TokenID.PRIMARY:                         return cfKeyword;
            case LTTS.Core.TokenID.PRINT:                           return cfKeyword;
            case LTTS.Core.TokenID.PROC:                            return cfKeyword;
            case LTTS.Core.TokenID.PROCEDURE:                       return cfKeyword;
            case LTTS.Core.TokenID.PROPERTY:                        return cfKeyword;
            case LTTS.Core.TokenID.RAISERROR:                       return cfKeyword;
            case LTTS.Core.TokenID.READ:                            return cfKeyword;
            case LTTS.Core.TokenID.READTEXT:                        return cfKeyword;
            case LTTS.Core.TokenID.RECONFIGURE:                     return cfKeyword;
            case LTTS.Core.TokenID.REFERENCES:                      return cfKeyword;
            case LTTS.Core.TokenID.REPLICATION:                     return cfKeyword;
            case LTTS.Core.TokenID.REQUIRED:                        return cfKeyword;
            case LTTS.Core.TokenID.RESTORE:                         return cfKeyword;
            case LTTS.Core.TokenID.RESTRICT:                        return cfKeyword;
            case LTTS.Core.TokenID.RETURN:                          return cfKeyword;
            case LTTS.Core.TokenID.RETURNS:                         return cfKeyword;
            case LTTS.Core.TokenID.REVERT:                          return cfKeyword;
            case LTTS.Core.TokenID.REVOKE:                          return cfKeyword;
            case LTTS.Core.TokenID.RIGHT:                           return cfKeyword;
            case LTTS.Core.TokenID.ROLLBACK:                        return cfKeyword;
            case LTTS.Core.TokenID.ROWGUIDCOL:                      return cfKeyword;
            case LTTS.Core.TokenID.RULE:                            return cfKeyword;
            case LTTS.Core.TokenID.SAVE:                            return cfKeyword;
            case LTTS.Core.TokenID.SCHEMA:                          return cfKeyword;
            case LTTS.Core.TokenID.SECURITYAUDIT:                   return cfKeyword;
            case LTTS.Core.TokenID.SELECT:                          return cfKeyword;
            case LTTS.Core.TokenID.SET:                             return cfKeyword;
            case LTTS.Core.TokenID.SETUSER:                         return cfKeyword;
            case LTTS.Core.TokenID.SHUTDOWN:                        return cfKeyword;
            case LTTS.Core.TokenID.SOME:                            return cfKeyword;
            case LTTS.Core.TokenID.SOURCE:                          return cfKeyword;
            case LTTS.Core.TokenID.STATIC:                          return cfKeyword;
            case LTTS.Core.TokenID.STATISTICS:                      return cfKeyword;
            case LTTS.Core.TokenID.TABLE:                           return cfKeyword;
            case LTTS.Core.TokenID.TABLESAMPLE:                     return cfKeyword;
            case LTTS.Core.TokenID.TEXTSIZE:                        return cfKeyword;
            case LTTS.Core.TokenID.THEN:                            return cfKeyword;
            case LTTS.Core.TokenID.THROW:                           return cfKeyword;
            case LTTS.Core.TokenID.TO:                              return cfKeyword;
            case LTTS.Core.TokenID.TRAN:                            return cfKeyword;
            case LTTS.Core.TokenID.TRANSACTION:                     return cfKeyword;
            case LTTS.Core.TokenID.TRIGGER:                         return cfKeyword;
            case LTTS.Core.TokenID.TRUNCATE:                        return cfKeyword;
            case LTTS.Core.TokenID.TRY:                             return cfKeyword;
            case LTTS.Core.TokenID.TSEQUAL:                         return cfKeyword;
            case LTTS.Core.TokenID.TYPE:                            return cfKeyword;
            case LTTS.Core.TokenID.UNION:                           return cfKeyword;
            case LTTS.Core.TokenID.UNIQUE:                          return cfKeyword;
            case LTTS.Core.TokenID.UNPIVOT:                         return cfKeyword;
            case LTTS.Core.TokenID.UPDATE:                          return cfKeyword;
            case LTTS.Core.TokenID.UPDATETEXT:                      return cfKeyword;
            case LTTS.Core.TokenID.USE:                             return cfKeyword;
            case LTTS.Core.TokenID.VALUES:                          return cfKeyword;
            case LTTS.Core.TokenID.VARYING:                         return cfKeyword;
            case LTTS.Core.TokenID.VIEW:                            return cfKeyword;
            case LTTS.Core.TokenID.WAITFOR:                         return cfKeyword;
            case LTTS.Core.TokenID.WHEN:                            return cfKeyword;
            case LTTS.Core.TokenID.WHERE:                           return cfKeyword;
            case LTTS.Core.TokenID.WHILE:                           return cfKeyword;
            case LTTS.Core.TokenID.WITH:                            return cfKeyword;
            case LTTS.Core.TokenID.WRITETEXT:                       return cfKeyword;
            }

            return null;
        }
    }
}