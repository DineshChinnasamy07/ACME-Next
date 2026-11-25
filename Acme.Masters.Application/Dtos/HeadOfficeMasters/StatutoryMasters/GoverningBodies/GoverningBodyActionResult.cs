/*  Class Name      : GoverningBodyActionResult.cs
 *  Purpose         : Result of Governing Body member save / delete
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.GoverningBodies
{
    public class GoverningBodyActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int ExecutiveId { get; set; }
    }
}
