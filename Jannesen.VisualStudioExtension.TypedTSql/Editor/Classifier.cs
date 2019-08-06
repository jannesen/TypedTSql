using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using LTTS_Core = Jannesen.Language.TypedTSql.Core;
using LTTS_DM   = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor
{
    internal class Classifier: ExtensionBase, IClassifier
    {
        public      event       EventHandler<ClassificationChangedEventArgs>    ClassificationChanged;

        private     readonly    IClassificationType                             _cfComment;
        private     readonly    IClassificationType                             _cfName;
        private     readonly    IClassificationType                             _cfNumber;
        private     readonly    IClassificationType                             _cfString;
        private     readonly    IClassificationType                             _cfOperator;
        private     readonly    IClassificationType                             _cfKeyword;
        private     readonly    IClassificationType                             _cfLocalVariable;
        private     readonly    IClassificationType                             _cfGlobalVariable;
        private     readonly    IClassificationType                             _cfBuildIn;
        private     readonly    IClassificationType                             _cfType;
        private     readonly    IClassificationType                             _cfTable;
        private     readonly    IClassificationType                             _cfView;
        private     readonly    IClassificationType                             _cfFunction;
        private     readonly    IClassificationType                             _cfStoredProcedure;
        private     readonly    IClassificationType                             _cfParameter;
        private     readonly    IClassificationType                             _cfColumn;
        private     readonly    IClassificationType                             _cfUDTValue;

        public                                                                  Classifier(IServiceProvider serviceProvider, ITextBuffer textBuffer, IClassificationTypeRegistryService registry) : base(serviceProvider, textBuffer)
        {
            _cfComment         = registry.GetClassificationType(ClassifierClassificationTypes.Comment);
            _cfName            = registry.GetClassificationType(ClassifierClassificationTypes.Name);
            _cfNumber          = registry.GetClassificationType(ClassifierClassificationTypes.Number);
            _cfString          = registry.GetClassificationType(ClassifierClassificationTypes.String);
            _cfOperator        = registry.GetClassificationType(ClassifierClassificationTypes.Operator);
            _cfKeyword         = registry.GetClassificationType(ClassifierClassificationTypes.Keyword);
            _cfLocalVariable   = registry.GetClassificationType(ClassifierClassificationTypes.LocalVariable);
            _cfGlobalVariable  = registry.GetClassificationType(ClassifierClassificationTypes.GlobalVariable);
            _cfBuildIn         = registry.GetClassificationType(ClassifierClassificationTypes.BuildIn);
            _cfType            = registry.GetClassificationType(ClassifierClassificationTypes.Type);
            _cfTable           = registry.GetClassificationType(ClassifierClassificationTypes.Table);
            _cfView            = registry.GetClassificationType(ClassifierClassificationTypes.View);
            _cfFunction        = registry.GetClassificationType(ClassifierClassificationTypes.Function);
            _cfStoredProcedure = registry.GetClassificationType(ClassifierClassificationTypes.StoredProcedure);
            _cfParameter       = registry.GetClassificationType(ClassifierClassificationTypes.Parameter);
            _cfColumn          = registry.GetClassificationType(ClassifierClassificationTypes.Column);
            _cfUDTValue        = registry.GetClassificationType(ClassifierClassificationTypes.UDTValue);
        }

        public                  void                                            OnTranspileDone(ITextSnapshot snapshot)
        {
            ClassificationChanged?.Invoke(this, new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
        }
        public                  IList<ClassificationSpan>                       GetClassificationSpans(SnapshotSpan span)
        {

            var currentSnapshot = TextBuffer.CurrentSnapshot;
            var fileResult      = GetFileResult();
            var rtn             = new List<ClassificationSpan>();

            if (fileResult != null && fileResult.Tokens != null) {
                try {
                    var         selectSpan = currentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive).GetSpan(fileResult.Snapshot);
                    int         firstSpanToken = 0;
                    int         bsL = 0;
                    int         bsR = fileResult.Tokens.Count -1;

                    while (bsL < bsR - 2) {
                        var bsM = bsL + (bsR-bsL)/2;
                        var t   = fileResult.Tokens[bsM];

                        if (t.Ending.Filepos < selectSpan.Start) {
                            bsL = bsM + 1;
                            continue;
                        }

                        if (t.Beginning.Filepos > selectSpan.Start) {
                            bsR = bsM - 1;
                            continue;
                        }

                        firstSpanToken = bsM-1;
                        if (firstSpanToken < 0)
                            firstSpanToken = 0;
                        break;
                    }

                    for(int i = firstSpanToken ; i < fileResult.Tokens.Count ; ++i) {
                        LTTS_Core.Token token        = fileResult.Tokens[i];
                        SnapshotSpan    snapshotSpan = CreateSpan(fileResult.Snapshot, token.Beginning.Filepos, token.Ending.Filepos);

                        if (span.IntersectsWith(snapshotSpan)) {
                            IClassificationType     ct = _tokenClassificationType(token);

                            if (ct != null)
                                rtn.Add(new ClassificationSpan(new SnapshotSpan(fileResult.Snapshot, token.Beginning.Filepos, token.Ending.Filepos-token.Beginning.Filepos), ct));
                        }
                        else {
                            if (token.Beginning.Filepos > selectSpan.End)
                                break;
                        }
                    }
                }
                catch(Exception) {
                    // snapshot.CreateTrackingSpan some times failed with a ArgumentOutOfRangeException exception. Just ignore is the best solution.
                }
            }

            return rtn;
        }

        private                 IClassificationType                             _tokenClassificationType(LTTS_Core.Token token)
        {
            LTTS_DM.ISymbol symbol = (token as LTTS_Core.TokenWithSymbol)?.Symbol;

            if (symbol != null) {
                switch(symbol.Type) {
                case LTTS_DM.SymbolType.BuildinFunction:                        return _cfBuildIn;
                case LTTS_DM.SymbolType.TypeUser:                               return _cfType;
                case LTTS_DM.SymbolType.TypeExternal:                           return _cfType;
                case LTTS_DM.SymbolType.TypeTable:                              return _cfType;
                case LTTS_DM.SymbolType.TableInternal:                          return _cfTable;
                case LTTS_DM.SymbolType.TableSystem:                            return _cfTable;
                case LTTS_DM.SymbolType.TableUser:                              return _cfTable;
                case LTTS_DM.SymbolType.View:                                   return _cfView;
                case LTTS_DM.SymbolType.Function:                               return _cfFunction;
                case LTTS_DM.SymbolType.FunctionScalar:                         return _cfFunction;
                case LTTS_DM.SymbolType.FunctionScalar_clr:                     return _cfFunction;
                case LTTS_DM.SymbolType.FunctionInlineTable:                    return _cfFunction;
                case LTTS_DM.SymbolType.FunctionMultistatementTable:            return _cfFunction;
                case LTTS_DM.SymbolType.FunctionMultistatementTable_clr:        return _cfFunction;
                case LTTS_DM.SymbolType.FunctionAggregateFunction_clr:          return _cfFunction;
                case LTTS_DM.SymbolType.StoredProcedure:                        return _cfStoredProcedure;
                case LTTS_DM.SymbolType.StoredProcedure_clr:                    return _cfStoredProcedure;
                case LTTS_DM.SymbolType.StoredProcedure_extended:               return _cfStoredProcedure;
                case LTTS_DM.SymbolType.Parameter:                              return _cfParameter;
                case LTTS_DM.SymbolType.Column:                                 return _cfColumn;
                case LTTS_DM.SymbolType.UDTValue:                               return _cfUDTValue;
                }
            }

            switch(token.ID) {
            case LTTS_Core.TokenID.BlockComment:                    return _cfComment;
            case LTTS_Core.TokenID.LineComment:                     return _cfComment;
            case LTTS_Core.TokenID.Name:                            return token.isKeyword ? _cfKeyword : _cfName;
            case LTTS_Core.TokenID.QuotedName:                      return _cfName;
            case LTTS_Core.TokenID.LocalName:                       return (token.Text.StartsWith("@@", StringComparison.InvariantCulture) ? _cfGlobalVariable : _cfLocalVariable);
            case LTTS_Core.TokenID.String:                          return _cfString;
            case LTTS_Core.TokenID.Number:                          return _cfNumber;
            case LTTS_Core.TokenID.BinaryValue:                     return _cfNumber;

            case LTTS_Core.TokenID.Equal:                           return _cfOperator;
            case LTTS_Core.TokenID.NotEqual:                        return _cfOperator;
            case LTTS_Core.TokenID.Greater:                         return _cfOperator;
            case LTTS_Core.TokenID.Less:                            return _cfOperator;

            case LTTS_Core.TokenID.Star:                            return _cfOperator;
            case LTTS_Core.TokenID.Divide:                          return _cfOperator;
            case LTTS_Core.TokenID.Module:                          return _cfOperator;
            case LTTS_Core.TokenID.Plus:                            return _cfOperator;
            case LTTS_Core.TokenID.Minus:                           return _cfOperator;
            case LTTS_Core.TokenID.BitNot:                          return _cfOperator;
            case LTTS_Core.TokenID.BitOr:                           return _cfOperator;
            case LTTS_Core.TokenID.BitAnd:                          return _cfOperator;
            case LTTS_Core.TokenID.BitXor:                          return _cfOperator;
            case LTTS_Core.TokenID.LessEqual:                       return _cfOperator;
            case LTTS_Core.TokenID.GreaterEqual:                    return _cfOperator;
            case LTTS_Core.TokenID.PlusAssign:                      return _cfOperator;
            case LTTS_Core.TokenID.MinusAssign:                     return _cfOperator;
            case LTTS_Core.TokenID.MultAssign:                      return _cfOperator;
            case LTTS_Core.TokenID.DivAssign:                       return _cfOperator;
            case LTTS_Core.TokenID.ModAssign:                       return _cfOperator;
            case LTTS_Core.TokenID.AndAssign:                       return _cfOperator;
            case LTTS_Core.TokenID.XorAssign:                       return _cfOperator;
            case LTTS_Core.TokenID.OrAssign:                        return _cfOperator;

            case LTTS_Core.TokenID.AND:                             return _cfOperator;
            case LTTS_Core.TokenID.BETWEEN:                         return _cfOperator;
            case LTTS_Core.TokenID.IN:                              return _cfOperator;
            case LTTS_Core.TokenID.EXISTS:                          return _cfOperator;
            case LTTS_Core.TokenID.NOT:                             return _cfOperator;
            case LTTS_Core.TokenID.NULL:                            return _cfOperator;
            case LTTS_Core.TokenID.OR:                              return _cfOperator;

            case LTTS_Core.TokenID.ADD:                             return _cfKeyword;
            case LTTS_Core.TokenID.ALL:                             return _cfKeyword;
            case LTTS_Core.TokenID.ALTER:                           return _cfKeyword;
            case LTTS_Core.TokenID.ANY:                             return _cfKeyword;
            case LTTS_Core.TokenID.APPLY:                           return _cfKeyword;
            case LTTS_Core.TokenID.AS:                              return _cfKeyword;
            case LTTS_Core.TokenID.ASC:                             return _cfKeyword;
            case LTTS_Core.TokenID.ASSEMBLY:                        return _cfKeyword;
            case LTTS_Core.TokenID.AUTHORIZATION:                   return _cfKeyword;
            case LTTS_Core.TokenID.BACKUP:                          return _cfKeyword;
            case LTTS_Core.TokenID.BEGIN:                           return _cfKeyword;
            case LTTS_Core.TokenID.BREAK:                           return _cfKeyword;
            case LTTS_Core.TokenID.BROWSE:                          return _cfKeyword;
            case LTTS_Core.TokenID.BULK:                            return _cfKeyword;
            case LTTS_Core.TokenID.BY:                              return _cfKeyword;
            case LTTS_Core.TokenID.CASCADE:                         return _cfKeyword;
            case LTTS_Core.TokenID.CASE:                            return _cfKeyword;
            case LTTS_Core.TokenID.CAST:                            return _cfKeyword;
            case LTTS_Core.TokenID.CATCH:                           return _cfKeyword;
            case LTTS_Core.TokenID.CHECK:                           return _cfKeyword;
            case LTTS_Core.TokenID.CHECKPOINT:                      return _cfKeyword;
            case LTTS_Core.TokenID.CLOSE:                           return _cfKeyword;
            case LTTS_Core.TokenID.CLUSTERED:                       return _cfKeyword;
            case LTTS_Core.TokenID.COALESCE:                        return _cfKeyword;
            case LTTS_Core.TokenID.COLLATE:                         return _cfKeyword;
            case LTTS_Core.TokenID.COLUMN:                          return _cfKeyword;
            case LTTS_Core.TokenID.COMMIT:                          return _cfKeyword;
            case LTTS_Core.TokenID.COMPUTE:                         return _cfKeyword;
            case LTTS_Core.TokenID.CONSTRAINT:                      return _cfKeyword;
            case LTTS_Core.TokenID.CONTAINS:                        return _cfKeyword;
            case LTTS_Core.TokenID.CONTAINSTABLE:                   return _cfKeyword;
            case LTTS_Core.TokenID.CONTINUE:                        return _cfKeyword;
            case LTTS_Core.TokenID.CONVERT:                         return _cfKeyword;
            case LTTS_Core.TokenID.CREATE:                          return _cfKeyword;
            case LTTS_Core.TokenID.CROSS:                           return _cfKeyword;
            case LTTS_Core.TokenID.CURRENT:                         return _cfKeyword;
            case LTTS_Core.TokenID.CURSOR:                          return _cfKeyword;
            case LTTS_Core.TokenID.DATABASE:                        return _cfKeyword;
            case LTTS_Core.TokenID.DBCC:                            return _cfKeyword;
            case LTTS_Core.TokenID.DEALLOCATE:                      return _cfKeyword;
            case LTTS_Core.TokenID.DECLARE:                         return _cfKeyword;
            case LTTS_Core.TokenID.DEFAULT:                         return _cfKeyword;
            case LTTS_Core.TokenID.DELETE:                          return _cfKeyword;
            case LTTS_Core.TokenID.DENY:                            return _cfKeyword;
            case LTTS_Core.TokenID.DEPENDENTASSEMBLY:               return _cfKeyword;
            case LTTS_Core.TokenID.DESC:                            return _cfKeyword;
            case LTTS_Core.TokenID.DISK:                            return _cfKeyword;
            case LTTS_Core.TokenID.DISTINCT:                        return _cfKeyword;
            case LTTS_Core.TokenID.DISTRIBUTED:                     return _cfKeyword;
            case LTTS_Core.TokenID.DOUBLE:                          return _cfKeyword;
            case LTTS_Core.TokenID.DROP:                            return _cfKeyword;
            case LTTS_Core.TokenID.DUMP:                            return _cfKeyword;
            case LTTS_Core.TokenID.ELSE:                            return _cfKeyword;
            case LTTS_Core.TokenID.END:                             return _cfKeyword;
            case LTTS_Core.TokenID.ERRLVL:                          return _cfKeyword;
            case LTTS_Core.TokenID.ESCAPE:                          return _cfKeyword;
            case LTTS_Core.TokenID.EXCEPT:                          return _cfKeyword;
            case LTTS_Core.TokenID.EXEC:                            return _cfKeyword;
            case LTTS_Core.TokenID.EXECUTE:                         return _cfKeyword;
            case LTTS_Core.TokenID.EXIT:                            return _cfKeyword;
            case LTTS_Core.TokenID.EXTERNAL:                        return _cfKeyword;
            case LTTS_Core.TokenID.FETCH:                           return _cfKeyword;
            case LTTS_Core.TokenID.FILE:                            return _cfKeyword;
            case LTTS_Core.TokenID.FILLFACTOR:                      return _cfKeyword;
            case LTTS_Core.TokenID.FOR:                             return _cfKeyword;
            case LTTS_Core.TokenID.FOREIGN:                         return _cfKeyword;
            case LTTS_Core.TokenID.FREETEXT:                        return _cfKeyword;
            case LTTS_Core.TokenID.FREETEXTTABLE:                   return _cfKeyword;
            case LTTS_Core.TokenID.FROM:                            return _cfKeyword;
            case LTTS_Core.TokenID.FULL:                            return _cfKeyword;
            case LTTS_Core.TokenID.FUNCTION:                        return _cfKeyword;
            case LTTS_Core.TokenID.GOTO:                            return _cfKeyword;
            case LTTS_Core.TokenID.GRANT:                           return _cfKeyword;
            case LTTS_Core.TokenID.GROUP:                           return _cfKeyword;
            case LTTS_Core.TokenID.HANDLERCONFIG:                   return _cfKeyword;
            case LTTS_Core.TokenID.HAVING:                          return _cfKeyword;
            case LTTS_Core.TokenID.HOLDLOCK:                        return _cfKeyword;
            case LTTS_Core.TokenID.IDENTITY:                        return _cfKeyword;
            case LTTS_Core.TokenID.IDENTITY_INSERT:                 return _cfKeyword;
            case LTTS_Core.TokenID.IDENTITYCOL:                     return _cfKeyword;
            case LTTS_Core.TokenID.IF:                              return _cfKeyword;
            case LTTS_Core.TokenID.INDEX:                           return _cfKeyword;
            case LTTS_Core.TokenID.INNER:                           return _cfKeyword;
            case LTTS_Core.TokenID.INSERT:                          return _cfKeyword;
            case LTTS_Core.TokenID.INTERSECT:                       return _cfKeyword;
            case LTTS_Core.TokenID.INTO:                            return _cfKeyword;
            case LTTS_Core.TokenID.IS:                              return _cfKeyword;
            case LTTS_Core.TokenID.JOIN:                            return _cfKeyword;
            case LTTS_Core.TokenID.KEY:                             return _cfKeyword;
            case LTTS_Core.TokenID.KILL:                            return _cfKeyword;
            case LTTS_Core.TokenID.LEFT:                            return _cfKeyword;
            case LTTS_Core.TokenID.LIKE:                            return _cfKeyword;
            case LTTS_Core.TokenID.LINENO:                          return _cfKeyword;
            case LTTS_Core.TokenID.LOAD:                            return _cfKeyword;
            case LTTS_Core.TokenID.MERGE:                           return _cfKeyword;
            case LTTS_Core.TokenID.METHOD:                          return _cfKeyword;
            case LTTS_Core.TokenID.NATIONAL:                        return _cfKeyword;
            case LTTS_Core.TokenID.NOCHECK:                         return _cfKeyword;
            case LTTS_Core.TokenID.NONCLUSTERED:                    return _cfKeyword;
            case LTTS_Core.TokenID.OF:                              return _cfKeyword;
            case LTTS_Core.TokenID.OFF:                             return _cfKeyword;
            case LTTS_Core.TokenID.OFFSETS:                         return _cfKeyword;
            case LTTS_Core.TokenID.ON:                              return _cfKeyword;
            case LTTS_Core.TokenID.OPEN:                            return _cfKeyword;
            case LTTS_Core.TokenID.OPTION:                          return _cfKeyword;
            case LTTS_Core.TokenID.ORDER:                           return _cfKeyword;
            case LTTS_Core.TokenID.OUTER:                           return _cfKeyword;
            case LTTS_Core.TokenID.OVER:                            return _cfKeyword;
            case LTTS_Core.TokenID.PERCENT:                         return _cfKeyword;
            case LTTS_Core.TokenID.PIVOT:                           return _cfKeyword;
            case LTTS_Core.TokenID.PLAN:                            return _cfKeyword;
            case LTTS_Core.TokenID.PRECISION:                       return _cfKeyword;
            case LTTS_Core.TokenID.PRIMARY:                         return _cfKeyword;
            case LTTS_Core.TokenID.PRINT:                           return _cfKeyword;
            case LTTS_Core.TokenID.PROC:                            return _cfKeyword;
            case LTTS_Core.TokenID.PROCEDURE:                       return _cfKeyword;
            case LTTS_Core.TokenID.PROPERTY:                        return _cfKeyword;
            case LTTS_Core.TokenID.RAISERROR:                       return _cfKeyword;
            case LTTS_Core.TokenID.READ:                            return _cfKeyword;
            case LTTS_Core.TokenID.READTEXT:                        return _cfKeyword;
            case LTTS_Core.TokenID.RECONFIGURE:                     return _cfKeyword;
            case LTTS_Core.TokenID.REFERENCES:                      return _cfKeyword;
            case LTTS_Core.TokenID.REPLICATION:                     return _cfKeyword;
            case LTTS_Core.TokenID.REQUIRED:                        return _cfKeyword;
            case LTTS_Core.TokenID.RESTORE:                         return _cfKeyword;
            case LTTS_Core.TokenID.RESTRICT:                        return _cfKeyword;
            case LTTS_Core.TokenID.RETURN:                          return _cfKeyword;
            case LTTS_Core.TokenID.RETURNS:                         return _cfKeyword;
            case LTTS_Core.TokenID.REVERT:                          return _cfKeyword;
            case LTTS_Core.TokenID.REVOKE:                          return _cfKeyword;
            case LTTS_Core.TokenID.RIGHT:                           return _cfKeyword;
            case LTTS_Core.TokenID.ROLLBACK:                        return _cfKeyword;
            case LTTS_Core.TokenID.ROWGUIDCOL:                      return _cfKeyword;
            case LTTS_Core.TokenID.RULE:                            return _cfKeyword;
            case LTTS_Core.TokenID.SAVE:                            return _cfKeyword;
            case LTTS_Core.TokenID.SCHEMA:                          return _cfKeyword;
            case LTTS_Core.TokenID.SECURITYAUDIT:                   return _cfKeyword;
            case LTTS_Core.TokenID.SELECT:                          return _cfKeyword;
            case LTTS_Core.TokenID.SET:                             return _cfKeyword;
            case LTTS_Core.TokenID.SETUSER:                         return _cfKeyword;
            case LTTS_Core.TokenID.SHUTDOWN:                        return _cfKeyword;
            case LTTS_Core.TokenID.SOME:                            return _cfKeyword;
            case LTTS_Core.TokenID.SOURCE:                          return _cfKeyword;
            case LTTS_Core.TokenID.STATIC:                          return _cfKeyword;
            case LTTS_Core.TokenID.STATISTICS:                      return _cfKeyword;
            case LTTS_Core.TokenID.TABLE:                           return _cfKeyword;
            case LTTS_Core.TokenID.TABLESAMPLE:                     return _cfKeyword;
            case LTTS_Core.TokenID.TEXTSIZE:                        return _cfKeyword;
            case LTTS_Core.TokenID.THEN:                            return _cfKeyword;
            case LTTS_Core.TokenID.THROW:                           return _cfKeyword;
            case LTTS_Core.TokenID.TO:                              return _cfKeyword;
            case LTTS_Core.TokenID.TRAN:                            return _cfKeyword;
            case LTTS_Core.TokenID.TRANSACTION:                     return _cfKeyword;
            case LTTS_Core.TokenID.TRIGGER:                         return _cfKeyword;
            case LTTS_Core.TokenID.TRUNCATE:                        return _cfKeyword;
            case LTTS_Core.TokenID.TRY:                             return _cfKeyword;
            case LTTS_Core.TokenID.TSEQUAL:                         return _cfKeyword;
            case LTTS_Core.TokenID.TYPE:                            return _cfKeyword;
            case LTTS_Core.TokenID.UNION:                           return _cfKeyword;
            case LTTS_Core.TokenID.UNIQUE:                          return _cfKeyword;
            case LTTS_Core.TokenID.UNPIVOT:                         return _cfKeyword;
            case LTTS_Core.TokenID.UPDATE:                          return _cfKeyword;
            case LTTS_Core.TokenID.UPDATETEXT:                      return _cfKeyword;
            case LTTS_Core.TokenID.USE:                             return _cfKeyword;
            case LTTS_Core.TokenID.VALUES:                          return _cfKeyword;
            case LTTS_Core.TokenID.VARYING:                         return _cfKeyword;
            case LTTS_Core.TokenID.VIEW:                            return _cfKeyword;
            case LTTS_Core.TokenID.WAITFOR:                         return _cfKeyword;
            case LTTS_Core.TokenID.WHEN:                            return _cfKeyword;
            case LTTS_Core.TokenID.WHERE:                           return _cfKeyword;
            case LTTS_Core.TokenID.WHILE:                           return _cfKeyword;
            case LTTS_Core.TokenID.WITH:                            return _cfKeyword;
            case LTTS_Core.TokenID.WRITETEXT:                       return _cfKeyword;
            }

            return null;
        }
    }

    [Export(typeof(IClassifierProvider)), ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName)]
    internal class ClassifierProvider: IClassifierProvider
    {
#pragma warning disable 0649
        [Import]
        private                     IClassificationTypeRegistryService          classificationRegistry;
        [Import]
        private                     SVsServiceProvider                          ServiceProvider;
#pragma warning restore 0649

        public                      IClassifier                                 GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty<Classifier>(typeof(Classifier), () => new Classifier(ServiceProvider, textBuffer, this.classificationRegistry));
        }
    }
}
