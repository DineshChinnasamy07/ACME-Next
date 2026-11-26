/*  Class Name      : TdsSectionActionResult.cs
 *  Purpose         : Save / delete outcome for a TDS Section
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.TdsSection
{
    public class TdsSectionActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TdsSectionId { get; set; }
    }
}
