/*  Class Name      : TdsPolicyActionResult.cs
 *  Purpose         : TDS Policy save / delete outcome
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsPolicy
{
    public class TdsPolicyActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TdsPolicyId { get; set; }
    }
}
