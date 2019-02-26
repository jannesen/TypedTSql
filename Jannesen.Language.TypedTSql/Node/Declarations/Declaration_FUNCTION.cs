using System;
using System.Collections.Generic;
using System.IO;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms186755.aspx
    // Scalar Function
    //      CREATE FUNCTION Objectname
    //      ( [ { @parameter_name [ AS ] Datatype [ = default ] [ READONLY ] }  [ ,...n ] ] )
    //      RETURNS return_data_type
    //      [ WITH <function_option> [ ,...n ] ]
    //      [ AS ]
    //      BEGIN
    //          function_body
    //          RETURN scalar_expression
    //      END
    //
    // Inline Table-Valued
    //      CREATE FUNCTION Objectname
    //      ( [ { @parameter_name [ AS ] Datatype [ = default ] [ READONLY ] }  [ ,...n ] ] )
    //      RETURNS TABLE
    //      [ WITH <function_option> [ ,...n ] ]
    //      [ AS ]
    //      RETURN [ ( ] select_stmt [ ) ]
    //
    // Multistatement Table-valued
    //      CREATE FUNCTION Objectname
    //      ( [ { @parameter_name [ AS ] [ type_schema_name. ] parameter_data_type [ = default ] [READONLY] } [ ,...n ] ] )
    //      RETURNS @return_variable TABLE <table_type_definition>
    //      [ WITH <function_option> [ ,...n ] ]
    //      [ AS ]
    //      BEGIN
    //          function_body
    //          RETURN
    //      END
    [DeclarationParser(Core.TokenID.FUNCTION)]
    public class Declaration_FUNCTION: DeclarationObjectCode
    {
        public      override    DataModel.SymbolType            EntityType                  { get { return _functionType;       } }
        public      override    DataModel.EntityName            EntityName                  { get { return n_Name.n_EntitiyName;  } }
        public      override    bool                            callableFromCode            { get { return true; } }

        public      readonly    Node_EntityNameDefine           n_Name;
        public      readonly    Core.TokenWithSymbol            n_ReturnVariable;
        public      readonly    Core.AstParseNode               n_ReturnType;
        public      readonly    Node_External                   n_Node_External;

        public      override    ObjectReturnOption              ReturnOption                { get { return _functionType == DataModel.SymbolType.FunctionScalar ? ObjectReturnOption.Required : ObjectReturnOption.Nothing;  } }
        public      override    DataModel.ISqlType              ReturnType                  { get { return _returnType;                                                                                                      } }
        public      override    DataModel.VariableLocal         ReturnVariable              { get { return _returnVariable;                                                                                                  } }

        private                 DataModel.SymbolType            _functionType;
        private                 DataModel.ISqlType              _returnType;
        private                 DataModel.VariableLocal         _returnVariable;

        public                                                  Declaration_FUNCTION(Core.ParserReader reader, IParseContext parseContext)
        {
            AddLeading(reader);
            AddChild(new Node.Node_CustomNode("CREATE "));
            ParseToken(reader, Core.TokenID.FUNCTION);
            n_Name = AddChild(new Node_EntityNameDefine(reader));
            ParseParameters(reader, Node_SqlParameter.InterfaceType.Function);
            ParseToken(reader, Core.TokenID.RETURNS);

            if (reader.CurrentToken.isToken(Core.TokenID.LocalName)) {
                _functionType = DataModel.SymbolType.FunctionMultistatementTable;
                n_ReturnVariable = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.LocalName);
                ParseToken(reader, Core.TokenID.TABLE);
                n_ReturnType = AddChild(new Table(reader, TableType.Variable));
            }
            else if (ParseOptionalToken(reader, Core.TokenID.TABLE) != null)
            {
                _functionType = DataModel.SymbolType.FunctionInlineTable;
            }
            else {
                _functionType = DataModel.SymbolType.FunctionScalar;
                n_ReturnType = AddChild(new Node_Datatype(reader));
            }

            ParseWith(reader, DataModel.SymbolType.Function);
            ParseGrant(reader, DataModel.SymbolType.Function);

            ParseOptionalToken(reader, Core.TokenID.AS);

            switch(_functionType) {
            case DataModel.SymbolType.FunctionScalar:
            case DataModel.SymbolType.FunctionMultistatementTable:
                switch(reader.CurrentToken.validateToken(Core.TokenID.BEGIN, Core.TokenID.EXTERNAL)) {
                case Core.TokenID.BEGIN:
                    ParseStatementBlock(reader, false);
                    break;

                case Core.TokenID.EXTERNAL:
                    switch(_functionType) {
                    case DataModel.SymbolType.FunctionScalar:               _functionType = DataModel.SymbolType.FunctionScalar_clr;                break;
                    case DataModel.SymbolType.FunctionMultistatementTable:  _functionType = DataModel.SymbolType.FunctionMultistatementTable_clr;   break;
                    }

                    n_Node_External = AddChild(new Node_External(reader));
                    break;
                }
                break;

            case DataModel.SymbolType.FunctionInlineTable:
                ParseOptionalToken(reader, Core.TokenID.RETURN);
                ParseOptionalToken(reader, Core.TokenID.LrBracket);
                ParseStatementQuery(reader, Query_SelectContext.FunctionInlineTable);
                ParseOptionalToken(reader, Core.TokenID.RrBracket);
                break;
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            if (!_declarationTranspiled) {
                _returnType = null;

                n_Name.TranspileNode(context);
                n_Parameters?.TranspileNode(context);
                TranspileOptions(context);

                switch(_functionType) {
                case DataModel.SymbolType.FunctionScalar:
                    n_ReturnType.TranspileNode(context);
                    _returnType = ((Node_Datatype)n_ReturnType).SqlType;
                    break;

                case DataModel.SymbolType.FunctionInlineTable: {
                        TranspileStatement(context, query:true);
                        _returnType = new DataModel.SqlTypeTable(Entity, ((Query_Select)n_Statement).Resultset?.GetUniqueNamedList(), null);
                    }
                    break;

                case DataModel.SymbolType.FunctionMultistatementTable: {
                        n_ReturnType.TranspileNode(context);
                        var table = (Table)n_ReturnType;
                        var returnType = new DataModel.SqlTypeTable(Entity, table.Columns, table.Indexes);
                        _returnType = returnType;
                        _returnVariable = new DataModel.VariableLocal(n_ReturnVariable.Text,
                                                                      _returnType,
                                                                       n_ReturnVariable,
                                                                       DataModel.VariableFlags.Returns);
                        n_ReturnVariable.SetSymbol(_returnVariable);

                        if (n_Parameters != null && n_Parameters.t_Parameters.Contains(_returnVariable.Name))
                            context.AddError(n_ReturnVariable, "Variable already defined.");
                    }
                    break;
                }

                if (_returnType != null) {
                    Entity.Transpiled(parameters: n_Parameters?.t_Parameters,
                                      returns:    _returnType);
                    n_Name.n_Name.SetSymbol(Entity);
                }

                _declarationTranspiled = true;
            }

            switch(_functionType) {
            case DataModel.SymbolType.FunctionInlineTable:
                break;

            default:
                if (n_Node_External != null)
                    n_Node_External.TranspileNode(context);
                else
                    TranspileStatement(context);
                break;
            }

            Transpiled = true;
        }
        public      override    void                            EmitDrop(StringWriter stringWriter)
        {
            stringWriter.Write("IF EXISTS (SELECT * FROM sys.sysobjects WHERE [id] = object_id(");
                stringWriter.Write(Library.SqlStatic.QuoteString(n_Name.n_EntitiyName.Fullname));
                stringWriter.WriteLine(") AND [type] in ('FN','IF','TF','AF', 'FS', 'FT'))");
            stringWriter.Write("    DROP FUNCTION ");
                stringWriter.WriteLine(n_Name.n_EntitiyName.Fullname);
        }

        public      override    Core.IAstNode                   GetNameToken()
        {
            return n_Name;
        }
        public      override    string                          CollapsedName()
        {
            return "function " + n_Name.n_EntitiyName.Name;
        }
    }
}
