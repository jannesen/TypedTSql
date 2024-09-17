using System;
using System.IO;

namespace Jannesen.Language.TypedTSql.WebService.Library
{
    internal static class FileHelpers
    {
        public  static      void            DeleteFilesDirectory(string directory, string extension, bool child=false)
        {
            if (child || Directory.Exists(directory)) {
                foreach (string path in Directory.GetDirectories(directory)) {
                    DeleteFilesDirectory(path, extension, true);
                    DeleteDirectory(path);
                }

                foreach(var filename in Directory.GetFiles(directory, extension, SearchOption.TopDirectoryOnly))
                    DeleteFile(filename);
            }
        }
        public  static      void            DeleteDirectory(string name)
        {
            for (int i = 0 ;  ; ++i) {
                try {
                    Directory.Delete(name);
                    return;
                }
               catch(IOException err) {
                    if (err is FileNotFoundException || err is DirectoryNotFoundException)
                        return;

                    if (i > 100 || !_errorretry(err.HResult)) {
                        throw new Exception("Can't delete '" + name + "'.", err);
                    }
                }

                System.Threading.Thread.Sleep(100);
            }
        }
        public  static      void            DeleteFile(string name)
        {
            for (int i = 0 ;  ; ++i) {
                try {
                    File.Delete(name);
                    return;
                }
               catch(IOException err) {
                    if (err is FileNotFoundException || err is DirectoryNotFoundException)
                        return;

                    if (i > 100 || !_errorretry(err.HResult)) {
                        throw new Exception("Can't delete '" + name + "'.", err);
                    }
                }

                System.Threading.Thread.Sleep(100);
            }
        }
        private static      bool            _errorretry(int hresult)
        {
            return hresult == unchecked((int)0x80070020);
        }

    }
}
