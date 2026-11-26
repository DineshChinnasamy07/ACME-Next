/*  Class Name      : MasterInstiTan.cs
 *  Purpose         : HO tenant MASTER_INSTI_TAN row - one TAN (number / register date / certificate name) of a
 *                    Legal Entity; a legal entity can hold more than one TAN.
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class MasterInstiTan
    {
        public int TanId { get; set; }

        public int CustomerId { get; set; }

        public string TanNo { get; set; } = string.Empty;

        public DateTime? TanRegDate { get; set; }

        public string? TanName { get; set; }
    }
}
