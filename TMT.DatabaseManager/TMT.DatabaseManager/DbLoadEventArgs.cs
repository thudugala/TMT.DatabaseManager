namespace TMT.DatabaseManager
{
    public class DbLoadEventArgs
    {
        public DbLoadEventArgs()
        {
            this.UseBackticks = true;
        }

        public string[] ColumnDbNameList { get; set; }
        public string DefaultOrderByStatement { get; set; }
        public string DefaultWhereStatement { get; set; }
        public bool IsCaseSensitive { get; set; }
        public int? LimitOffset { get; set; }
        public bool LoadSchema { get; set; }
        public DbSearchCondition[] SearchConditionList { get; set; }
        public string TableName { get; set; }
        public bool UseBackticks { get; set; }
    }
}