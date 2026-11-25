/*  Class Name      : LegalEntityOptionDto.cs
 *  Purpose         : Id / name option row (association nature, denomination, legal entity lookups)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class LegalEntityOptionDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
