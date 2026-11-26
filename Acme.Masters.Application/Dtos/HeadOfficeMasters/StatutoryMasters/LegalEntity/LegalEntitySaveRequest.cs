/*  Class Name      : LegalEntitySaveRequest.cs
 *  Purpose         : Legal Entity add (CustomerId = 0) / update request
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 *  Modified on     : 26-Nov-2026 - Society master statutory fields and TAN list
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class LegalEntitySaveRequest
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

        // Society master sheet (26-Nov-2026) - stored in MASTER_INSTI_STATUTORY / MASTER_INSTI_TAN.

        /// <summary>1 Society, 2 Trust.</summary>
        public int? EntityType { get; set; }

        public string? TrustDocNo { get; set; }

        public string? BranchIncharge { get; set; }

        /// <summary>1 Obtained, 0 Not Obtained.</summary>
        public int? PanStatus { get; set; }

        public string? PanName { get; set; }

        public DateTime? PanRegDate { get; set; }

        /// <summary>1 Registered, 0 Not Registered (same for DARPAN, 12A, 80G, FCRA).</summary>
        public int? DarpanStatus { get; set; }

        public string? DarpanId { get; set; }

        public DateTime? DarpanRegDate { get; set; }

        public string? DarpanName { get; set; }

        /// <summary>All TANs of the legal entity (TANNO keeps the first one for older readers).</summary>
        public List<LegalEntityTanDto> Tans { get; set; } = new List<LegalEntityTanDto>();

        public int? A12Status { get; set; }

        public DateTime? A12ValidFrom { get; set; }

        public DateTime? A12ValidTo { get; set; }

        public string? A12Name { get; set; }

        public int? EightyGStatus { get; set; }

        public DateTime? EightyGValidFrom { get; set; }

        public DateTime? EightyGValidTo { get; set; }

        public string? EightyGName { get; set; }

        public int? FcraStatus { get; set; }

        public DateTime? FcraValidFrom { get; set; }

        public DateTime? FcraValidTo { get; set; }
    }
}
