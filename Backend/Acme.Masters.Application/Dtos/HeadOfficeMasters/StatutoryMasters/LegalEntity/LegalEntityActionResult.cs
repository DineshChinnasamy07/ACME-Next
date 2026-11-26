/*  Class Name      : LegalEntityActionResult.cs
 *  Purpose         : Result of Legal Entity save / delete
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
namespace Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class LegalEntityActionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int CustomerId { get; set; }
    }
}
