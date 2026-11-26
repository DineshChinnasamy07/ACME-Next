/*  Class Name      : MasterTdsDeducteeType.cs
 *  Purpose         : HO tenant TDS_DEDUCTEE_TYPE row (TDS deductee type master)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.DeducteeType
{
    public class MasterTdsDeducteeType
    {
        public int DeducteeTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ResidentialStatus { get; set; }
        public int DeducteeType { get; set; }
        public int Status { get; set; }
    }
}
