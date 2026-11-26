/*  Class Name      : LegalEntityTanDto.cs
 *  Purpose         : One TAN of a Legal Entity (save request and edit form data)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class LegalEntityTanDto
    {
        public string? TanNo { get; set; }

        public DateTime? TanRegDate { get; set; }

        public string? TanName { get; set; }
    }
}
