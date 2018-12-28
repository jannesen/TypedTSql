using System;
using System.Reflection;
using Jannesen.Language.TypedTSql.DataModel;
using Jannesen.Language.TypedTSql.Internal;

namespace Jannesen.Language.TypedTSql.BuildIn
{
    internal static class Catalog
    {
        public  static      VariableList                        GlobalVariable;
        public  static      BuildinFunctionDeclarationList      RowSetFunctions;
        public  static      BuildinFunctionDeclarationList      ScalarFunctions;

        public  static      void                                Load()
        {
        }

                static                                          Catalog()
        {
            GlobalVariable = new DataModel.VariableList(
                                new VariableGlobal("@@CONNECTIONS",     SqlTypeNative.Int),
                                new VariableGlobal("@@CPU_BUSY",        SqlTypeNative.Int),
                                new VariableGlobal("@@CURSOR_ROWS",     SqlTypeNative.Int),
                                new VariableGlobal("@@DATEFIRST",       SqlTypeNative.Int),
                                new VariableGlobal("@@DBTS",            new SqlTypeNative(SystemType.VarBinary, maxLength:8)),
                                new VariableGlobal("@@ERROR",           SqlTypeNative.Int),
                                new VariableGlobal("@@FETCH_STATUS",    SqlTypeNative.Int),
                                new VariableGlobal("@@IDENTITY",        SqlTypeNative.Int),
                                new VariableGlobal("@@IDLE",            SqlTypeNative.Int),
                                new VariableGlobal("@@IO_BUSY",         SqlTypeNative.Int),
                                new VariableGlobal("@@LANGID",          SqlTypeNative.Int),
                                new VariableGlobal("@@LANGUAGE",        new SqlTypeNative(SystemType.NVarChar, maxLength:256)),
                                new VariableGlobal("@@LOCK_TIMEOUT",    SqlTypeNative.Int),
                                new VariableGlobal("@@MAX_CONNECTIONS", SqlTypeNative.Int),
                                new VariableGlobal("@@MAX_PRECISION",   SqlTypeNative.Int),
                                new VariableGlobal("@@NESTLEVEL",       SqlTypeNative.Int),
                                new VariableGlobal("@@OPTIONS",         SqlTypeNative.Int),
                                new VariableGlobal("@@PACKET_ERRORS",   SqlTypeNative.Int),
                                new VariableGlobal("@@PACK_RECEIVED",   SqlTypeNative.Int),
                                new VariableGlobal("@@PACK_SENT",       SqlTypeNative.Int),
                                new VariableGlobal("@@PROCID",          SqlTypeNative.Int),
                                new VariableGlobal("@@ROWCOUNT",        SqlTypeNative.Int),
                                new VariableGlobal("@@SERVERNAME",      new SqlTypeNative(SystemType.NVarChar, maxLength:256)),
                                new VariableGlobal("@@SERVICENAME",     new SqlTypeNative(SystemType.NVarChar, maxLength:256)),
                                new VariableGlobal("@@SPID",            SqlTypeNative.Int),
                                new VariableGlobal("@@TEXTSIZE",        SqlTypeNative.Int),
                                new VariableGlobal("@@TIMETICKS",       SqlTypeNative.Int),
                                new VariableGlobal("@@TOTAL_ERRORS",    SqlTypeNative.Int),
                                new VariableGlobal("@@TOTAL_READ",      SqlTypeNative.Int),
                                new VariableGlobal("@@TOTAL_WRITE",     SqlTypeNative.Int),
                                new VariableGlobal("@@TRANCOUNT",       SqlTypeNative.Int),
                                new VariableGlobal("@@VERSION",         new SqlTypeNative(SystemType.NVarChar, maxLength:256))
                            );

            ScalarFunctions = new BuildinFunctionDeclarationList(200);
            RowSetFunctions = new BuildinFunctionDeclarationList(10);

            var types = Assembly.GetCallingAssembly().GetTypes();

            foreach(var type in types) {
                if (type.IsClass && type.IsPublic) {
                    switch(type.Namespace)
                    {
                    case "Jannesen.Language.TypedTSql.BuildIn.Func":
                        ScalarFunctions.Add(new BuildinFunctionDeclaration(type, false));
                        break;

                    case "Jannesen.Language.TypedTSql.BuildIn.RowSet":
                        RowSetFunctions.Add(new BuildinFunctionDeclaration(type, true));
                        break;
                    }
                }
            }
        }
    }
}
