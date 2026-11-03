/*  Class Name      : HeadOfficeActionResult.cs
 *  Purpose         : Success text for Head Office actions
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
namespace Bosco.Organization.Application.Dtos
{
    public class HeadOfficeActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int HeadOfficeId { get; set; }

        /// <summary>
        /// Plain admin password after first activate (until mail is added). Null otherwise.
        /// </summary>
        public string? GeneratedAdminPassword { get; set; }
    }
}
