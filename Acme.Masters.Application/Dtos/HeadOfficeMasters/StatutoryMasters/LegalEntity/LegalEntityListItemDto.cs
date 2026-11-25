/*  Class Name      : LegalEntityListItemDto.cs
 *  Purpose         : Legal Entity grid row (legacy LegalEntityView: Society Name, Reg No, Reg Date)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class LegalEntityListItemDto
    {
        public int CustomerId { get; set; }

        public string SocietyName { get; set; } = string.Empty;

        public string RegNo { get; set; } = string.Empty;

        public DateTime? RegDate { get; set; }

        public string Place { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;
    }
}
