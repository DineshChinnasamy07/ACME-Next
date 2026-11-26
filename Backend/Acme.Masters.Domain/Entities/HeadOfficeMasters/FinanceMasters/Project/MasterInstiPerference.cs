/*  Class Name      : MasterInstiPerference.cs
 *  Purpose         : HO tenant MASTER_INSTI_PERFERENCE row - the Legal Entity (Society).
 *                    Used as the Society lookup by Project and maintained by Statutory Masters > Legal Entity.
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 *  Modified on     : 24-Nov-2026 - Legal Entity columns added (Statutory Masters)
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.Project
{
    public class MasterInstiPerference
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

        public string? AssociationNature { get; set; }

        public string? OtherAssociationNature { get; set; }

        public int? Denomination { get; set; }

        public string? OtherDenomination { get; set; }

        public int IsFoundation { get; set; }

        public string? PrincipalActivity { get; set; }
    }
}
