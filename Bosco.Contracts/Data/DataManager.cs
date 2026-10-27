/*  Class Name      : DataManager.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using Bosco.Common;

namespace Bosco.Contracts.Data
{
    public class DataManager
    {
        private DataParameters dataParameters = new DataParameters();
        private DataCommandArguments dataCommandArgs = new DataCommandArguments();
        private ResultArgs resultArgs = null;

        public DataManager()
            : this("", "")
        {
        }

        public DataManager(DataBaseType databaseType)
            : this("", databaseType)
        {
            this.DataCommandArgs.ActiveDatabaseType = databaseType;
        }

        public DataManager(object sqlCommandId)
            : this(sqlCommandId, "")
        {
        }

        public DataManager(object sqlCommandId, string tableName)
        {
            this.DataCommandArgs.SQLCommandId = sqlCommandId;
            this.DataCommandArgs.TableName = tableName;
        }

        public DataManager(object sqlCommandId, DataBaseType databaseType)
            : this(sqlCommandId, "", databaseType)
        {
            this.DataCommandArgs.ActiveDatabaseType = databaseType;
        }

        public DataManager(object sqlCommandId, DataBaseType databaseType, SQLAdapterType sqladapter)
            : this(sqlCommandId, "", databaseType)
        {
            this.DataCommandArgs.ActiveSQLAdapterType = sqladapter;
            this.DataCommandArgs.ActiveDatabaseType = databaseType;
        }

        public DataManager(object sqlCommandId, SQLAdapterType sqladapter)
            : this(sqlCommandId, "")
        {
            this.DataCommandArgs.ActiveSQLAdapterType = sqladapter;
        }

        public DataManager(object sqlCommandId, string tableName, DataBaseType databaseType)
            : this(sqlCommandId, tableName)
        {
            this.DataCommandArgs.ActiveDatabaseType = databaseType;
        }

        public DataCommandArguments DataCommandArgs
        {
            get { return dataCommandArgs; }
        }

        public virtual DataParameters Parameters
        {
            get { return dataParameters; }
        }

        public virtual ResultArgs Result
        {
            get { return resultArgs; }
            set { resultArgs = value; }
        }
    }
}
