using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jannesen.VisualStudioExtension.TypedTSql.Build.Library
{
    static class Statics
    {
        public  static          bool                ReadFileStatus(BinaryReader binaryReader, string filename)
        {
            Int64   length = binaryReader.ReadInt64();
            Int64   ticks  = binaryReader.ReadInt64();

            try {
                FileInfo    fileInfo = new FileInfo(filename);

                return fileInfo.Length != length || fileInfo.LastWriteTimeUtc.Ticks != ticks;
            }
            catch(Exception ) {
                return true;
            }
        }
        public  static          void                SaveFileStatus(BinaryWriter binaryWriter, string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);

            binaryWriter.Write(filename);
            binaryWriter.Write(fileInfo.Length);
            binaryWriter.Write(fileInfo.LastWriteTimeUtc.Ticks);
        }
        public  static          string              NormelizeFullPath(string path)
        {
            var     parts = new List<string>(path.Split(new char[] { '/', '\\'}));
            int     rootlength;

            if (parts[0].Length == 2 && parts[0][1] == ':')
                rootlength = 1;
            else
            if (parts.Count > 3 && parts[0].Length == 0 && parts[1].Length == 0)
                rootlength = 4;
            else
                throw new ArgumentException("Invallid full path '" + path + "'.");

            for (int i = 0 ; i < parts.Count ; ) {
                switch(parts[i]) {
                case ".":
                    parts.RemoveAt(i);
                    break;

                case "..":
                    if (rootlength > i-1)
                        throw new ArgumentException("Invallid full path '" + path + "'.");

                    parts.RemoveRange(i-1, 2);
                    i -= 1;
                    break;

                default:
                    ++i;
                    break;
                }
            }

            var rtn = new StringBuilder();

            for (int i = 0 ; i < parts.Count ; ++i) {
                if (i > 0)
                    rtn.Append('\\');

                rtn.Append(parts[i]);
            }

            return rtn.ToString();
        }
    }
}
