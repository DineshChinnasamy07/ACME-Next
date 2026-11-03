/*  Class Name      : HeadOfficeType.cs
 *  Purpose         : Portal head_office_type row for type dropdown
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
namespace Bosco.Organization.Domain.Entities
{
    public class HeadOfficeType
    {
        public int TypeId { get; set; }

        public string Type { get; set; } = string.Empty;
    }
}
