/*  Class Name      : DeducteeTypeActionResult.cs
 *  Purpose         : Save / delete outcome for a Deductee Type
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.DeducteeType
{
    public class DeducteeTypeActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int DeducteeTypeId { get; set; }
    }
}
