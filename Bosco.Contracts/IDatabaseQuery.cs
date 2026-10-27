/*  Class Name      : IDatabaseQuery.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using Bosco.Contracts.Data;

namespace Bosco.Contracts
{
    public interface IDatabaseQuery
    {
        string GetQuery(DataCommandArguments dataCommandArgs, ref SQLType sqlType);
    }
}
