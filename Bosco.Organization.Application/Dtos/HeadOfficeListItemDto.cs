/*  Class Name      : HeadOfficeListItemDto.cs
 *  Purpose         : One row for Head Office list (FetchHeadOfficeDetails)
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Organization.Application.Dtos
{
    public class HeadOfficeListItemDto
    {
        public int HeadOfficeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? BelongsTo { get; set; }
        public int? TypeId { get; set; }
        public int Status { get; set; }
        public string HoStatus { get; set; } = string.Empty;
    }
}