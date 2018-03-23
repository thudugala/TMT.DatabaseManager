using System;
using System.Collections.Generic;

namespace TMT.DatabaseManager
{
    public class DbSearchCondition
    {
        public string DbColumnName { get; set; }
        public bool IsFuntion { get; set; }
        public string SearchValues { get; set; }
        public Type SearchValuesType => Type.GetType(this.SearchValuesTypeName);
        public string SearchValuesTypeName { get; set; }

        public IList<string> GetSearchValueList()
        {
            return SearchValues?.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public override string ToString()
        {
            return $"[{SearchValues}], {SearchValuesType}, {DbColumnName}, {IsFuntion}";
        }
    }
}