/*  Class Name      : BranchOfficeActionResult.cs
 *  Purpose         : Success text for Branch Office actions
 *  Author          : Dinesh C
 *  Created on      : 02-Nov-2026
 */
namespace Bosco.Organization.Application.Dtos
{
    public class BranchOfficeActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int BranchOfficeId { get; set; }
    }
}
