using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jannesen.Language.TypedTSql.Logic
{
    public static class LogicStatic
    {
        public  static  DataModel.ValueFlags    ComputedValueFlags(DataModel.ValueFlags valueFlags)
        {
            return (valueFlags & (DataModel.ValueFlags.Error|DataModel.ValueFlags.Nullable|DataModel.ValueFlags.SourceFlags|DataModel.ValueFlags.Collate|DataModel.ValueFlags.Aggregaat)) | DataModel.ValueFlags.Computed;
        }
        public  static  DataModel.ValueFlags    FunctionValueFlags(DataModel.ValueFlags valueFlags)
        {
            return (valueFlags & (DataModel.ValueFlags.Error|DataModel.ValueFlags.Nullable|DataModel.ValueFlags.SourceFlags|DataModel.ValueFlags.Collate|DataModel.ValueFlags.Aggregaat)) | DataModel.ValueFlags.Function;
        }
    }
}
