using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using Microsoft.VisualStudio.Shell.FindAllReferences;
using Microsoft.VisualStudio.Shell.TableManager;
using LTTS                 = Jannesen.Language.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.FindAllReferences
{
    internal class ReferenceEntry: ITableEntry
    {
        public              object                                  Identity => this;

        private readonly    FindAllReferenceWindow                  _window;
        private readonly    object                                  _definition;
        private readonly    object                                  _projectName;
        private readonly    object                                  _filename;
        private readonly    object                                  _line;
        private readonly    object                                  _column;
        private readonly    object                                  _containing;
        private readonly    object                                  _usage;
        private             LTTS.Core.Token[]                       _lineTokens;
        private             object                                  _text;
        private             object                                  _textInlines;

        public                                                      ReferenceEntry(FindAllReferenceWindow window, DefinitionBucket definition, string projectName, LTTS.SymbolReference symbolReference)
        {
            _window      = window;
            _definition  = definition;
            _projectName = projectName;
            _filename    = symbolReference.SourceFile.Filename;
            _line        = symbolReference.Token.Beginning.Lineno - 1;
            _column      = symbolReference.Token.Beginning.Linepos - 1;
            _lineTokens  = symbolReference.GetLineTokens().ToArray();

            try {
                var usage = symbolReference.GetUsage();
                _containing = usage.Containing;
                _usage      = (usage.UsageFlags != LTTS.DataModel.SymbolUsageFlags.None) ? usage.UsageFlags.ToString() : null;
            }
            catch(Exception err) {
                _usage = "ERROR: " + err.Message;
            }
        }

        public              bool                                    TryGetValue(string keyName, out object content)
        {
            content = _getValue(keyName);
            return content != null;
        }
        public              bool                                    CanSetValue(string keyName)
        {
            return false;
        }
        public              bool                                    TrySetValue(string keyName, object content)
        {
           return false;
        }

        private             object                                  _getValue(string keyName)
        {
            switch (keyName) {
            case StandardTableKeyNames.Definition:          return _definition;
            case StandardTableKeyNames.DisplayPath:         return _filename;
            case StandardTableKeyNames.DocumentName:        return _filename;
            case StandardTableKeyNames.Line:                return _line;
            case StandardTableKeyNames.Column:              return _column;
            case StandardTableKeyNames.ProjectName:         return _projectName;
            case StandardTableKeyNames.Text:                return (_text == null) ? _text = LTTS.SymbolReference.GetLine(_lineTokens) : _text;
            case StandardTableKeyNames.TextInlines:         return (_textInlines == null) ? _textInlines = _window.FormatTextInlines(_lineTokens) : _textInlines;
            case ContainingColumnDefinition.ColumnName:     return _containing;
            case UsageColumnDefinition.ColumnName:          return _usage;

            case StandardTableKeyNames.DetailsExpander:
            case StandardTableKeyNames.HasVerticalContent:
            case StandardTableKeyNames.PersistentSpan:
            case StandardTableKeyNames.ProjectNames:
            case StandardTableKeyNames.HelpKeyword:
            case StandardTableKeyNames.HelpLink:
            case "IsTextTruncated":
            case "rawlinetext":
            case "header":
            case "resultIndex":
                return null;

            default:
#if DEBUG
                System.Diagnostics.Debug.WriteLine("ReferenceEntry.GetValue unknown key:" + keyName);
#endif
                return null;
            }
        }
    }
}
