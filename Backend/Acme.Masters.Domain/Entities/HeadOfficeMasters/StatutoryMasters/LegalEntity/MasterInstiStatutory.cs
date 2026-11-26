/*  Class Name      : MasterInstiStatutory.cs
 *  Purpose         : HO tenant MASTER_INSTI_STATUTORY row - Society master statutory details of one Legal Entity
 *                    (1:1 with MASTER_INSTI_PERFERENCE by CUSTOMERID): Society/Trust type, trust document,
 *                    in-charge, PAN, DARPAN, 12A, 80G and FCRA status / validity / certificate names.
 *                    Kept in its own table so existing MASTER_INSTI_PERFERENCE readers are unaffected.
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class MasterInstiStatutory
    {
        public int CustomerId { get; set; }

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
