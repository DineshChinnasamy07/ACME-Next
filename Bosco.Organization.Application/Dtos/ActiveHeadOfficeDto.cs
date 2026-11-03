/*  Class Name      : ActiveHeadOfficeDto.cs
 *  Purpose         : Activated Head Office row for dropdown (FetchActiveHeadOfficeDetails)
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */


namespace Bosco.Organization.Application.Dtos
{
    public class ActiveHeadOfficeDto
    {
        public int HeadOfficeId { get; set; }
        public string HeadOfficeCode { get; set; } = string.Empty;
    }
}
