using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Jannesen.Language.TypedTSql.Library
{
    public static class FileUpdate
    {
        public      static          void        Update(string filename, MemoryStream data)
        {
            Update(filename, data.GetBuffer(), (int)data.Length);
        }
        public      static          void        Update(string filename, byte[] data, int length)
        {
            try {
                if (File.Exists(filename)) {
                    var cur = File.ReadAllBytes(filename);
                    if (cur.Length == length && memcmp(cur, data, length) == 0)
                        return;
                }
            }
            catch(Exception err) {
                System.Diagnostics.Debug.WriteLine("FileUpdate compare failed " + filename + ": " + err.Message);
            }

            using (var file = _openFile(filename))
                file.Write(data, 0, length);
        }
        private     static          FileStream  _openFile(string name)
        {
            int ntry = 0;

retry:
            try {
                return new FileStream(name, FileMode.Create);
            }
            catch(DirectoryNotFoundException) {
                _makePath(Path.GetDirectoryName(name));
                goto retry;
            }
            catch(UnauthorizedAccessException) {
                if (++ntry < 100)
                    goto retry;

                throw;
            }
        }
        private     static          void        _makePath(string name)
        {
            try {
                Directory.CreateDirectory(name);
            }
            catch(DirectoryNotFoundException) {
                _makePath(Path.GetDirectoryName(name));
                Directory.CreateDirectory(name);
            }
        }

        [DllImport("msvcrt.dll", CallingConvention=CallingConvention.Cdecl)]
        private     static extern   int         memcmp(byte[] b1, byte[] b2, long count);
    }
}
