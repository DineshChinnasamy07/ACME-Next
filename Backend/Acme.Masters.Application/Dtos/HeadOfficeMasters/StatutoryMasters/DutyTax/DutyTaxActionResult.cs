/*  Class Name      : DutyTaxActionResult.cs
 *  Purpose         : Save / delete outcome for a Duty / Tax type
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DutyTax
{
    public class DutyTaxActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TdsDutyTaxTypeId { get; set; }
    }
}
