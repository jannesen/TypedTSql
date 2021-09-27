using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LTTS            = Jannesen.Language.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.Build
{
    class Tester
    {
        static      void        Main(string[] args)
        {
            try {
                var database = args[0];

                for (int i = 1 ; i < args.Length ; ++i)
                    (new Tester()).Run(database, args[i]);
            }
            catch(Exception err) {
                while (err != null) {
                    System.Diagnostics.Debug.WriteLine("ERROR: " + err.Message);
                    Console.WriteLine("ERROR: " + err.Message);
                    err = err.InnerException;
                }
            }
        }

        public                  Tester()
        {

        }

        public      void        Run(string databasename, string directory)
        {
            try {
                Console.WriteLine(directory);

                using (var outputStream = new StreamWriter(directory + "\\output.txt")) {
                    using (var database   = new LTTS.SqlDatabase(databasename)) {
                        outputStream.WriteLine("========== SQL CODE");
                        database.Output(outputStream, true);

                        var transpiler = new LTTS.Transpiler();
                        transpiler.LoadExtensions("Jannesen.Language.TypedTSql.WebService");
                        transpiler.Parse(Directory.GetFiles(directory, "*.ttsql", SearchOption.AllDirectories));

                        if (transpiler.ErrorCount == 0)
                            transpiler.Transpile(new LTTS.GlobalCatalog(database));

                        if (transpiler.ErrorCount == 0) {
                            transpiler.Emit(new LTTS.EmitOptions()
                                                {
                                                    DontEmitComment       = true,
                                                    DontEmitCustomComment = true,
                                                    BaseDirectory         = directory
                                                },
                                            database,
                                            null);
                        }

                        if (transpiler.ErrorCount > 0) {
                            outputStream.WriteLine("========== ERRORS ");

                            foreach(var error in transpiler.Errors)
                                outputStream.WriteLine(error.SourceFile.Filename + "(" +  error.Beginning.Lineno + "," + error.Beginning.Linepos + "," + error.Ending.Lineno + "," + error.Ending.Linepos + "): " + error.Message);
                        }
                        else
                            outputStream.WriteLine("========== NO ERRORS ");
                    }
                }
            }
            catch(Exception err) {
                while (err != null) {
                    System.Diagnostics.Debug.WriteLine(directory + " ERROR: " + err.Message);
                    Console.WriteLine(directory + " ERROR: " + err.Message);
                    err = err.InnerException;
                }
            }
        }
    }
}
