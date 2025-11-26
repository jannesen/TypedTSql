using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

// Disable CS0618 warning for now.
#pragma warning disable CS0618

namespace Jannesen.VisualStudioExtension.TypedTSql.Classification
{
    static class ClassificationFormats
    {
        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.Comment)]
        [Name("TTSQL Comment"), DisplayName("TTSQL Comment")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatComment: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatComment(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.Comment);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.Name)]
        [Name("TTSQL Name"), DisplayName("TTSQL Name")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatName: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatName(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.Name);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.Number)]
        [Name("TTSQL Number"), DisplayName("TTSQL Number")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatNumber: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatNumber(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.Number);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.String)]
        [Name("TTSQL String"), DisplayName("TTSQL String")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierString: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierString(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.String);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.Operator)]
        [Name("TTSQL Operator"), DisplayName("TTSQL Operator")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatOperator: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatOperator(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.Operator);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.Keyword)]
        [Name("TTSQL Keyword"), DisplayName("TTSQL Keyword")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatKeyword: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatKeyword(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.Keyword);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.GlobalVariable)]
        [Name("TTSQL Global Variable"), DisplayName("TTSQL Global Variable")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatGlobalVariable: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatGlobalVariable(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.GlobalVariable);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.LocalVariable)]
        [Name("TTSQL Local Variable"), DisplayName("TTSQL Local Variable")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatLocalVariable: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatLocalVariable(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.LocalVariable);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.BuildIn)]
        [Name("TTSQL Buildin"), DisplayName("TTSQL Buildin")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatBuildIn: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatBuildIn(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.BuildIn);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.Type)]
        [Name("TTSQL Type"), DisplayName("TTSQL Type")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatType: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatType(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.Type);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.Table)]
        [Name("TTSQL Table"), DisplayName("TTSQL Table")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatTable: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatTable(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.Table);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.View)]
        [Name("TTSQL View"), DisplayName("TTSQL View")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatView: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatView(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.View);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.Function)]
        [Name("TTSQL Function"), DisplayName("TTSQL Function")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatFunction: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatFunction(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.Function);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.StoredProcedure)]
        [Name("TTSQL StoredProcedure"), DisplayName("TTSQL StoredProcedure")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatStoredProcedure: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatStoredProcedure(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.StoredProcedure);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.Parameter)]
        [Name("TTSQL Parameter"), DisplayName("TTSQL Parameter")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatParameter: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatParameter(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.Parameter);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.Column)]
        [Name("TTSQL Column"), DisplayName("TTSQL Column")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatColumn: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatColumn(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.Column);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.UDTValue)]
        [Name("TTSQL UDTValue"), DisplayName("TTSQL UDTValue")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatUDTValue: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatUDTValue(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.UDTValue);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition)), ClassificationType(ClassificationTypeNames = ClassificationTypes.Error)]
        [Name("TTSQL Error"), DisplayName("TTSQL Error")]
        [UserVisible(true), Order(Before = Priority.Default)]
        internal sealed class ClassifierFormatError: ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassifierFormatError(ColorManager colorManager)
            {
                var fontColor = colorManager.GetDefaultColors(ClassificationTypes.Error);
                ForegroundColor = fontColor.ForegroundColor;
                BackgroundColor = fontColor.BackgroundColor;
            }
        }
    }
}
