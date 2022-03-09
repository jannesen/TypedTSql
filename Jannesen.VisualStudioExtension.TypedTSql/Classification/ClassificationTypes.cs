using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Jannesen.VisualStudioExtension.TypedTSql.Classification
{
    static class ClassificationTypes
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
#pragma warning disable CS0169
#pragma warning disable IDE0051
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
#pragma warning restore IDE0051
#pragma warning restore CS0169
    }
}
