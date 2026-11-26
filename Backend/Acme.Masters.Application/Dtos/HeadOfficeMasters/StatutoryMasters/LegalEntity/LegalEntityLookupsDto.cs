/*  Class Name      : LegalEntityLookupsDto.cs
 *  Purpose         : Legal Entity form lookups (legacy rdAssociationNature / rdDenomination items)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class LegalEntityLookupsDto
    {
        public IReadOnlyList<LegalEntityOptionDto> AssociationNatures { get; set; } = Array.Empty<LegalEntityOptionDto>();

        public IReadOnlyList<LegalEntityOptionDto> Denominations { get; set; } = Array.Empty<LegalEntityOptionDto>();
    }
}
