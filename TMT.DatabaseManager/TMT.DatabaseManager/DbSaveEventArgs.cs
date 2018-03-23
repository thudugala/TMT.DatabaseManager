using System.Data;

namespace TMT.DatabaseManager
{
    public class DbSaveEventArgs
    {
        public DataSet ChangedTableSet { get; set; }
        public bool ContinueUpdateOnError { get; set; }
    }
}