using System;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class ContextBlock: ContextParent
    {
        public      override    ContextBlock                        BlockContext                { get { return this; } }

        public                  int                                 BlockId;
        public                  DataModel.VariableList              VariableList;

        internal                                                    ContextBlock(Context parent): base(parent)
        {
            BlockId = RootContext.BlockNr++;
        }

        public                  void                                EndBlock()
        {
            if (VariableList != null) {
                foreach(var v in VariableList) {
                    if (!v.isAssigned) {
                        AddWarning((Core.Token)v.Declaration, "Variable " + v.Name + " never assigned.");
                    }
                    else
                    if (!v.isUsed) {
                        AddWarning((Core.Token)v.Declaration, "Variable " + v.Name + " not used.");
                    }
                }
            }
        }
    }
}
