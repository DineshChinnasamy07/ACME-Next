/*  Class Name      : TransProperty.cs
 *  Purpose         : Reusable member for ACPP BuildingBlocks
 *  Author          : Dinesh C
 *  Created on      : 16-Oct-2026
 */
using System.Data;

namespace Bosco.Common.ConfigSetting
{
    public class TransProperty : CommonMember
    {
        public DataView TransInfo { get; set; }
        public DataView CashTransInfo { get; set; }
        public DataTable FixedDepositInfo { get; set; }
        public DataTable FixedDepositInterestInfo { get; set; }
        public DataSet CostCentreInfo { get; set; }
        public DataSet DenominationInfo { get; set; }
        public DataTable TDSPayment { get; set; }
        public DataTable TDSBooking { get; set; }
    }
}
