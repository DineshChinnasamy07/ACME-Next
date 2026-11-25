/*  Class Name      : DeducteeTypeSaveRequest.cs
 *  Purpose         : Insert / update request for a Deductee Type (DeducteeTypeId 0 = insert)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DeducteeType
{
    public class DeducteeTypeSaveRequest
    {
        public int DeducteeTypeId { get; set; }

        public string? Name { get; set; }

        // 0 = Resident, 1 = Non Resident
        public int ResidentialStatus { get; set; }

        // 0 = Company, 1 = Non Company
        public int DeducteeType { get; set; }

        // 1 = Active, 0 = Inactive
        public int Status { get; set; } = 1;
    }
}
