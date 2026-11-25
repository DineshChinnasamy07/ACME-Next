/*  Class Name      : VouchersController.cs
 *  Purpose         : Head Office Vouchers - Vouchers endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.Vouchers
{
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/vouchers")]
    [Authorize]
    public class VouchersController : ControllerBase
    {
    }
}
