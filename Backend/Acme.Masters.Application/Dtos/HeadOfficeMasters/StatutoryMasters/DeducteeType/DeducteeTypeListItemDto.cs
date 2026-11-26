/*  Class Name      : DeducteeTypeListItemDto.cs
 *  Purpose         : Deductee Type grid row / detail returned by api/deducteetypes
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DeducteeType
{
    public class DeducteeTypeListItemDto
    {
        public int DeducteeTypeId { get; set; }

        public string Name { get; set; } = string.Empty;

        // 0 = Resident, 1 = Non Resident (Acme.Common.TDSResidentialStatus)
        public int ResidentialStatus { get; set; }

        public string ResidentialStatusName { get; set; } = string.Empty;

        // 0 = Company, 1 = Non Company (Acme.Common.TDSDeducteeStatus)
        public int DeducteeType { get; set; }

        public string DeducteeTypeName { get; set; } = string.Empty;

        // 1 = Active, 0 = Inactive
        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;
    }
}
