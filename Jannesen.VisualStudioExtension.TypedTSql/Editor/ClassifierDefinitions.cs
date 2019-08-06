using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

// Disable CS0618 warning for now.
#pragma warning disable CS0618 

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor
{
    static class ClassifierClassificationTypes
    {
        public const string     Comment         = "typedtsql.comment";
        public const string     Name            = "typedtsql.name";
        public const string     Number          = "typedtsql.number";
        public const string     String          = "typedtsql.string";
        public const string     Operator        = "typedtsql.operator";
        public const string     Keyword         = "typedtsql.keyword";
        public const string     GlobalVariable  = "typedtsql.globalvariable";
        public const string     LocalVariable   = "typedtsql.localvariable";
        public const string     BuildIn         = "typedtsql.buildin";
        public const string     Type            = "typedtsql.type";
        public const string     Table           = "typedtsql.table";
        public const string     View            = "typedtsql.view";
        public const string     Function        = "typedtsql.function";
        public const string     StoredProcedure = "typedtsql.storedprocedure";
        public const string     Parameter       = "typedtsql.parameter";
        public const string     Column          = "typedtsql.column";
        public const string     UDTValue        = "typedtsql.udtvalue";

        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169
        [Export(typeof(ClassificationTypeDefinition)), Name(Comment)]
        private static      ClassificationTypeDefinition        typeComment;

        [Export(typeof(ClassificationTypeDefinition)), Name(Name)]
        private static      ClassificationTypeDefinition        typeName;

        [Export(typeof(ClassificationTypeDefinition)), Name(Number)]
        private static      ClassificationTypeDefinition        typeNumber;

        [Export(typeof(ClassificationTypeDefinition)), Name(String)]
        private static      ClassificationTypeDefinition        typeString;

        [Export(typeof(ClassificationTypeDefinition)), Name(Operator)]
        private static      ClassificationTypeDefinition        typeOperator;

        [Export(typeof(ClassificationTypeDefinition)), Name(Keyword)]
        private static      ClassificationTypeDefinition        typeKeyword;

        [Export(typeof(ClassificationTypeDefinition)), Name(GlobalVariable)]
        private static      ClassificationTypeDefinition        typeGlobalVariable;

        [Export(typeof(ClassificationTypeDefinition)), Name(LocalVariable)]
        private static      ClassificationTypeDefinition        typeLocalVariable;

        [Export(typeof(ClassificationTypeDefinition)), Name(BuildIn)]
        private static      ClassificationTypeDefinition        typeBuildIn;

        [Export(typeof(ClassificationTypeDefinition)), Name(Type)]
        private static      ClassificationTypeDefinition        typeType;

        [Export(typeof(ClassificationTypeDefinition)), Name(Table)]
        private static      ClassificationTypeDefinition        typeTable;

        [Export(typeof(ClassificationTypeDefinition)), Name(View)]
        private static      ClassificationTypeDefinition        typeView;

        [Export(typeof(ClassificationTypeDefinition)), Name(Function)]
        private static      ClassificationTypeDefinition        typeFunction;

        [Export(typeof(ClassificationTypeDefinition)), Name(StoredProcedure)]
        private static      ClassificationTypeDefinition        typeStoredProcedure;

        [Export(typeof(ClassificationTypeDefinition)), Name(Parameter)]
        private static      ClassificationTypeDefinition        typeParameter;

        [Export(typeof(ClassificationTypeDefinition)), Name(Column)]
        private static      ClassificationTypeDefinition        typeColumn;

        [Export(typeof(ClassificationTypeDefinition)), Name(UDTValue)]
        private static      ClassificationTypeDefinition        typeUDTValue;
#pragma warning restore 169
    }

    static class ClassificationFormats
    {
        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.Comment)]
        [Name("TTSQL Comment"), DisplayName("TTSQL Comment")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatComment: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatComment(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.Comment);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.Name)]
        [Name("TTSQL Name"), DisplayName("TTSQL Name")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatName: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatName(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.Name);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.Number)]
        [Name("TTSQL Number"), DisplayName("TTSQL Number")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatNumber: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatNumber(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.Number);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.String)]
        [Name("TTSQL String"), DisplayName("TTSQL String")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierString: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierString(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.String);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.Operator)]
        [Name("TTSQL Operator"), DisplayName("TTSQL Operator")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatOperator: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatOperator(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.Operator);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.Keyword)]
        [Name("TTSQL Keyword"), DisplayName("TTSQL Keyword")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatKeyword: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatKeyword(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.Keyword);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.GlobalVariable)]
        [Name("TTSQL Global Variable"), DisplayName("TTSQL Global Variable")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatGlobalVariable: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatGlobalVariable(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.GlobalVariable);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.LocalVariable)]
        [Name("TTSQL Local Variable"), DisplayName("TTSQL Local Variable")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatLocalVariable: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatLocalVariable(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.LocalVariable);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.BuildIn)]
        [Name("TTSQL Buildin"), DisplayName("TTSQL Buildin")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatBuildIn: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatBuildIn(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.BuildIn);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.Type)]
        [Name("TTSQL Type"), DisplayName("TTSQL Type")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatType: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatType(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.Type);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.Table)]
        [Name("TTSQL Table"), DisplayName("TTSQL Table")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatTable: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatTable(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.Table);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.View)]
        [Name("TTSQL View"), DisplayName("TTSQL View")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatView: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatView(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.View);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.Function)]
        [Name("TTSQL Function"), DisplayName("TTSQL Function")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatFunction: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatFunction(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.Function);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.StoredProcedure)]
        [Name("TTSQL StoredProcedure"), DisplayName("TTSQL StoredProcedure")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatStoredProcedure: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatStoredProcedure(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.StoredProcedure);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.Parameter)]
        [Name("TTSQL Parameter"), DisplayName("TTSQL Parameter")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatParameter: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatParameter(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.Parameter);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.Column)]
        [Name("TTSQL Column"), DisplayName("TTSQL Column")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatColumn: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatColumn(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.Column);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassifierClassificationTypes.UDTValue)]
        [Name("TTSQL UDTValue"), DisplayName("TTSQL UDTValue")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatUDTValue: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatUDTValue(ClassificationColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassifierClassificationTypes.UDTValue);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }
    }

    [Export]
    public sealed class ClassificationColorManager: IDisposable
    {
        internal struct ClassificationColor: IEquatable<ClassificationColor>
        {
            public readonly Color?                  ForegroundColor;
            public readonly Color?                  BackgroundColor;

            public                                  ClassificationColor(Color? foregroundColor = null, Color? backgroundColor = null)
            {
                ForegroundColor = foregroundColor;
                BackgroundColor = backgroundColor;
            }

            public  static      bool            operator == (ClassificationColor p1, ClassificationColor p2)
            {
			    return p1.ForegroundColor == p2.ForegroundColor &&
                       p1.BackgroundColor == p2.BackgroundColor;
            }
            public  static      bool            operator != (ClassificationColor p1, ClassificationColor p2)
            {
                return !(p1 == p2);
            }
            public  override    bool            Equals(object obj)
            {
                if (obj is ClassificationColor)
                    return this == (ClassificationColor)obj;

                return false;
            }
            public              bool            Equals(ClassificationColor o)
            {
                return this == o;
            }
            public  override    int             GetHashCode()
            {
                return (ForegroundColor.HasValue ? ForegroundColor.Value.GetHashCode() : 0) ^
                       (BackgroundColor.HasValue ? BackgroundColor.Value.GetHashCode() : 0);
            }
        }
        class DefaultClassificationColor
        {
            public readonly ClassificationColor     LightAndBlue;
            public readonly ClassificationColor     Dark;

            public                                  DefaultClassificationColor(ClassificationColor lightAndBlue, ClassificationColor dark)
            {
                LightAndBlue = lightAndBlue;
                Dark         = dark;
            }
        }

        private static readonly     Dictionary<string, DefaultClassificationColor>      _defaultColors = new Dictionary<string, DefaultClassificationColor>
                                    {
                                        { ClassifierClassificationTypes.Comment,         new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128,   0)), new ClassificationColor(Color.FromRgb( 87, 166,  74))) },
                                        { ClassifierClassificationTypes.Name,            new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0,   0)), new ClassificationColor(Color.FromRgb(220, 220, 220))) },
                                        { ClassifierClassificationTypes.Number,          new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(255,   0,   0)), new ClassificationColor(Color.FromRgb(214, 157, 133))) },
                                        { ClassifierClassificationTypes.String,          new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(255,   0,   0)), new ClassificationColor(Color.FromRgb(214, 157, 133))) },
                                        { ClassifierClassificationTypes.Operator,        new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(128, 128, 128)), new ClassificationColor(Color.FromRgb(180, 180, 180))) },
                                        { ClassifierClassificationTypes.Keyword,         new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0, 255)), new ClassificationColor(Color.FromRgb( 86, 156, 214))) },
                                        { ClassifierClassificationTypes.LocalVariable,   new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0,   0)), new ClassificationColor(Color.FromRgb(220, 220, 220))) },
                                        { ClassifierClassificationTypes.GlobalVariable,  new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0,   0)), new ClassificationColor(Color.FromRgb(220, 220, 220))) },
                                        { ClassifierClassificationTypes.BuildIn,         new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(255,   0, 255)), new ClassificationColor(Color.FromRgb(201, 117, 213))) },
                                        { ClassifierClassificationTypes.Type,            new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128, 168)), new ClassificationColor(Color.FromRgb( 78, 201, 176))) },
                                        { ClassifierClassificationTypes.Table,           new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128, 168)), new ClassificationColor(Color.FromRgb( 78, 201, 176))) },
                                        { ClassifierClassificationTypes.View,            new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128, 168)), new ClassificationColor(Color.FromRgb( 78, 201, 176))) },
                                        { ClassifierClassificationTypes.Function,        new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128, 168)), new ClassificationColor(Color.FromRgb( 78, 201, 176))) },
                                        { ClassifierClassificationTypes.StoredProcedure, new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128, 168)), new ClassificationColor(Color.FromRgb( 78, 201, 176))) },
                                        { ClassifierClassificationTypes.Parameter,       new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0,   0)), new ClassificationColor(Color.FromRgb(220, 220, 220))) },
                                        { ClassifierClassificationTypes.Column,          new DefaultClassificationColor(new ClassificationColor(Color.FromRgb( 32, 132,   0)), new ClassificationColor(Color.FromRgb(184, 215, 163))) },
                                        { ClassifierClassificationTypes.UDTValue,        new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0,   0)), new ClassificationColor(Color.FromRgb(220, 220, 220))) },
                                    };

        private                     IClassificationFormatMapService         _classificationFormatMapService;
        private                     IClassificationTypeRegistryService      _classificationTypeRegistry;
        private                     VSPackage.ColorTheme                    _currentTheme;

        [ImportingConstructor]
        public ClassificationColorManager(IClassificationFormatMapService classificationFormatMapService, IClassificationTypeRegistryService classificationTypeRegistry)
        {
            _classificationFormatMapService = classificationFormatMapService;
            _classificationTypeRegistry     = classificationTypeRegistry;
            _currentTheme                   = VSPackage.GetCurrentTheme();
//            VSColorTheme.ThemeChanged += _onThemeChanged;
        }

        public                      void                                    Dispose()
        {
//            VSColorTheme.ThemeChanged -= _onThemeChanged;
        }

        internal                    ClassificationColor                     GetDefaultColors(string category)
        {
            if (!_defaultColors.TryGetValue(category, out DefaultClassificationColor defaultColor))
                defaultColor = null;

            return _getDefaultClassificationColor(_currentTheme, defaultColor);
        }

/*
 * Not working correct with custom colors disable for now see also https://github.com/fsprojects/VisualFSharpPowerTools/pull/1480
        private                     void                                    _onThemeChanged(EventArgs e)
        {
            var newTheme = _getCurrentTheme();

            if (newTheme != Theme.Unknown && newTheme != _currentTheme) {
                _currentTheme = newTheme;

                var formatMap = _classificationFormatMapService.GetClassificationFormatMap(category: "text");

                try {
                    formatMap.BeginBatchUpdate();

                    foreach (var pair in _defaultColors) {
                        var classificationType = _classificationTypeRegistry.GetClassificationType(pair.Key);
                        FontColor   newColor   = _getDefaultFontColor(newTheme, pair.Value);
                        var         oldProp    = formatMap.GetTextProperties(classificationType);

                        var foregroundBrush    = newColor.ForegroundColor.HasValue ? new SolidColorBrush(newColor.ForegroundColor.Value) : null;
                        var backgroundBrush    = newColor.BackgroundColor.HasValue ? new SolidColorBrush(newColor.BackgroundColor.Value) : null;
                        var newProp            = TextFormattingRunProperties.CreateTextFormattingRunProperties(foregroundBrush,
                                                                                                                backgroundBrush,
                                                                                                                oldProp.Typeface,
                                                                                                                null,
                                                                                                                null,
                                                                                                                oldProp.TextDecorations,
                                                                                                                oldProp.TextEffects,
                                                                                                                oldProp.CultureInfo);
                        formatMap.SetTextProperties(classificationType, newProp);
                    }
                }
                finally {
                    formatMap.EndBatchUpdate();
                }
            }
        }
*/
        private static              ClassificationColor                     _getDefaultClassificationColor(VSPackage.ColorTheme theme, DefaultClassificationColor defaultColor)
        {
            if (defaultColor != null) {
                switch(theme) {
                case VSPackage.ColorTheme.Dark:
                    return defaultColor.Dark;

                case VSPackage.ColorTheme.Light:
                case VSPackage.ColorTheme.Blue:
                default:
                    return defaultColor.LightAndBlue;
                }
            }
            else {
                switch(theme) {
                case VSPackage.ColorTheme.Dark:
                    return new ClassificationColor(Colors.White);

                case VSPackage.ColorTheme.Light:
                case VSPackage.ColorTheme.Blue:
                default:
                    return new ClassificationColor(Colors.Black);
                }
            }
        }
    }
}
