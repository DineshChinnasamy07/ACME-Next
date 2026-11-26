/*  Class Name      : DutyTaxListItemDto.cs
 *  Purpose         : Duty / Tax type grid row / detail returned by api/dutytaxes
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DutyTax
{
    public class DutyTaxListItemDto
    {
        public int TdsDutyTaxTypeId { get; set; }

        public string TaxTypeName { get; set; } = string.Empty;

        // 1 = Active, 0 = Inactive
        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        // Built-in tax types 1-5 (TDS With PAN, TDS Without PAN, Surcharge, Ed Cess, Sec Ed Cess) are
        // referenced by id in the voucher TDS calculations, so they cannot be renamed or deleted.
        public bool IsSystem { get; set; }
    }
}
