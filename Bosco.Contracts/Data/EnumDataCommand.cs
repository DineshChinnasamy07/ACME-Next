/*  Class Name      : EnumDataCommand.cs
 *  Purpose         : Enum Data type for Data Level Access
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */

namespace Bosco.Contracts.Data
{
    public enum TransactionType
    {
        None,
        Commit,
        Rollback
    }

    public enum ExecutionMode
    {
        None,
        Success,
        Fail
    }

    public enum DataType
    {
        Boolean,
        Byte,
        Char,
        Date,
        DateTime, 
        Decimal, 
        Double,
        Int,
        Int16,
        Int32,
        Int64,
        SByte,
        Single, 
        String,
        TimeSpan, 
        UInt16,
        UInt32,
        UInt64,
        ByteArray,
        Varchar,
        None
    }

    public enum DataSource
    {
        DataSet,
        DataReader,
        DataTable,
        DataView,
        Scalar
    }

    public enum DataBaseType
    {
        Portal,
        HeadOffice,
        BranchOffice
    }

    public enum SQLAdapterType
    {
        BoscoSQL,
        HOSQL,
        PayrollSQL,
    }

    public enum SQLType
    {
        SQLStatic,
        SQLDynamic,
        SQLStoredProcedure
    }

    public enum SQLParameterType
    {
        Input,
        Output
    }

    public enum SQLConstraint
    {
        AutoIncrement,
        PrimaryKey,
        UniqueKey,
        None
    }
}