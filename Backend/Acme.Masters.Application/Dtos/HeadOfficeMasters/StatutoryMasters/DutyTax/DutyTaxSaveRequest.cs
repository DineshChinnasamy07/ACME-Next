/*  Class Name      : DutyTaxSaveRequest.cs
 *  Purpose         : Insert / update request for a Duty / Tax type (TdsDutyTaxTypeId 0 = insert)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DutyTax
{
    public class DutyTaxSaveRequest
    {
        public int TdsDutyTaxTypeId { get; set; }

        public string? TaxTypeName { get; set; }

        // 1 = Active, 0 = Inactive
        public int Status { get; set; } = 1;
    }
}
