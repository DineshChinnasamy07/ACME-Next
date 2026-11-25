/*  Class Name      : LegalEntityDetailDto.cs
 *  Purpose         : Legal Entity edit form data (legacy LegalEntitySystem.FillBankProperties)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class LegalEntityDetailDto
    {
        public int CustomerId { get; set; }

        public string? SocietyName { get; set; }

        public string? ContactPerson { get; set; }

        public string? Address { get; set; }

        public string? Place { get; set; }

        public string? State { get; set; }

        public string? Country { get; set; }

        public string? Pincode { get; set; }

        public string? Phone { get; set; }

        public string? Fax { get; set; }

        public string? Email { get; set; }

        public string? Url { get; set; }

        public string? RegNo { get; set; }

        public DateTime? RegDate { get; set; }

        public string? PermissionNo { get; set; }

        public DateTime? PermissionDate { get; set; }

        public string? A12No { get; set; }

        public string? PanNo { get; set; }

        public string? GirNo { get; set; }

        public string? TanNo { get; set; }

        public string? FcriNo { get; set; }

        public DateTime? FcriRegDate { get; set; }

        public string? EightyGNo { get; set; }

        public DateTime? EightyGNoRegDate { get; set; }

        /// <summary>
        /// Comma separated association nature ids (0 Cultural, 1 Economic, 2 Educational, 3 Religious, 4 Social, 5 Others).
        /// </summary>
        public string? AssociationNature { get; set; }

        public string? OtherAssociationNature { get; set; }

        /// <summary>
        /// 1 Hindu, 2 Sikh, 3 Muslim, 4 Christian, 5 Buddhist, 6 Others (only when Religious is selected).
        /// </summary>
        public int Denomination { get; set; }

        public string? OtherDenomination { get; set; }

        public bool IsFoundation { get; set; }

        public string? PrincipalActivity { get; set; }
    }
}
