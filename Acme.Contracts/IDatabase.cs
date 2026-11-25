/*  Class Name      : IDatabase.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System;
using Acme.Common;
using Acme.Contracts.Data;

namespace Acme.Contracts
{
    public interface IDatabase : IDisposable
    {
        ResultArgs HasConnectionEstablished();
        ResultArgs Execute(DataCommandArguments command, SQLType sqlType, bool showMessage = false);
        ResultArgs Execute(DataCommandArguments command, string sqlStatement, SQLType sqlType, bool showMessage = false);
        ResultArgs Fetch(DataCommandArguments command, DataSource dataSourceType, string sqlStatement, SQLType sqlType, ref object dataSource);

        void BeginTransaction();
        void EndTransaction();
        void RollBackTransaction();
        string PreviousTransactionSource { get; }
        ExecutionMode TransExecutionMode { set; }
    }
}
