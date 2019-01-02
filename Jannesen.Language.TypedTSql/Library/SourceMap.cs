using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Library
{
    public class SourceMapRemapEntry
    {
        public              FilePosition            SourceBeginning;
        public              FilePosition            SourceEnding;
        public              FilePosition            TargetBeginning;
        public              FilePosition            TargetEnding;

        public                                      SourceMapRemapEntry(FilePosition sourceBeginning, FilePosition sourceEnding, FilePosition targetBeginning, FilePosition targetEnding)
        {
            this.SourceBeginning = sourceBeginning;
            this.SourceEnding    = sourceEnding;
            this.TargetBeginning = targetBeginning;
            this.TargetEnding    = targetEnding;
        }
    }

    public class SourceMap: List<SourceMapRemapEntry>
    {
        public              string                  Filename            { get; private set; }
        public              int                     Lineno              { get; private set; }

        public                                      SourceMap(string filename, int lineno)
        {
            this.Filename = filename;
            this.Lineno   = lineno;
        }

        public              void                    AddFileRemap()
        {
            base.Add(new SourceMapRemapEntry(new FilePosition(Lineno), new FilePosition(int.MaxValue), new FilePosition(1), new FilePosition(int.MaxValue)));
        }
        public              void                    AddRemapEntry(FilePosition sourceBeginning, FilePosition sourceEnding, FilePosition targetBeginning, FilePosition targetEnding)
        {
            if (base.Count > 0) {
                SourceMapRemapEntry     cur = base[base.Count - 1];

                if (cur.SourceEnding == sourceBeginning && cur.TargetEnding == targetBeginning) {
                    cur.SourceEnding = sourceEnding;
                    cur.TargetEnding = targetEnding;
                    return;
                }
            }

            base.Add(new SourceMapRemapEntry(sourceBeginning, sourceEnding, targetBeginning, targetEnding));
        }

        public              int                     RemapTargetToSource(int line)
        {
            foreach(SourceMapRemapEntry pos in this) {
                if (pos.TargetBeginning.Lineno <= line && line <= pos.TargetEnding.Lineno)
                    return pos.SourceBeginning.Lineno + (line - pos.TargetBeginning.Lineno);
            }

            return Lineno;
        }
    }
}
