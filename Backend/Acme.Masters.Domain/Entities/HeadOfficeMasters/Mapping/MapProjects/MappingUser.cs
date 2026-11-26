/*  Class Name      : MappingUser.cs
 *  Purpose         : HO tenant USER_INFO lookup row for Project Mapping (user in charge)
 *  Author          : Dinesh C
 *  Created on      : 26-Nov-2026
 */
namespace Acme.Masters.Domain.Entities.HeadOfficeMasters.Mapping.MapProjects
{
    public class MappingUser
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string? BranchCode { get; set; }

        public int Status { get; set; }

        public int? UserType { get; set; }
    }
}
