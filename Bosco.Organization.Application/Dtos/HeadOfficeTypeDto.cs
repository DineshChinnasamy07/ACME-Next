/*  Class Name      : HeadOfficeTypeDto.cs
 *  Purpose         : One row for Head Office type dropdown (FetchHeadOfficeType)
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
namespace Bosco.Organization.Application.Dtos
{
    public class HeadOfficeTypeDto
    {
        public int TypeId { get; set; }

        public string Type { get; set; } = string.Empty;
    }
}
