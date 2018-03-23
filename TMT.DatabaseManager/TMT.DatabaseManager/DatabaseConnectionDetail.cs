using System;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace TMT.DatabaseManager
{
    public class DatabaseConnectionDetail
    {
        private Type adapterType;
        private Type commandBuilderType;
        private Type connectionType;
        private Type parameterType;

        public Type AdapterType
        {
            get
            {
                return adapterType;
            }
            set
            {
                if (value.BaseType.Equals(typeof(DbDataAdapter)) == false)
                {
                    throw new ArgumentException(nameof(value), "Must be type of DbDataAdapter");
                }
                adapterType = value;
            }
        }

        public Type CommandBuilderType
        {
            get
            {
                return commandBuilderType;
            }
            set
            {
                if (value.BaseType.Equals(typeof(DbCommandBuilder)) == false)
                {
                    throw new ArgumentException(nameof(value), "Must be type of DbCommandBuilder");
                }
                commandBuilderType = value;
            }
        }

        public string ConnectionString { get; set; }

        public Type ConnectionType
        {
            get
            {
                return connectionType;
            }
            set
            {
                if (value.BaseType.Equals(typeof(DbConnection)) == false)
                {
                    throw new ArgumentException(nameof(value), "Must be type of DbConnection");
                }
                connectionType = value;
            }
        }

        public CultureInfo Culture { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseServerName { get; set; }
        public int DatabaseServerPort { get; set; }
        public bool IsMysql { get; set; }
        public string LogInPassword { get; set; }
        public string LogInUserId { get; set; }

        public Type ParameterType
        {
            get
            {
                return parameterType;
            }
            set
            {
                if (value.BaseType.Equals(typeof(DbParameter)) == false)
                {
                    throw new ArgumentException(nameof(value), "Must be type of DbParameter");
                }
                parameterType = value;
            }
        }

        private Assembly SQLClientAssembly { get; set; }
    }
}