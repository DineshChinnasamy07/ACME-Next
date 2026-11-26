/*  Class Name      : TdsPolicyOptionDto.cs
 *  Purpose         : TDS Policy lookup option (id, name, extra detail)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class TdsPolicyOptionDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;
    }
}
