/*  Class Name      : GeneralateOpeningBalanceController.cs
 *  Purpose         : Head Office Mapping - GeneralateOpeningBalance endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.Mapping
{
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/generalateopeningbalance")]
    [Authorize]
    public class GeneralateOpeningBalanceController : ControllerBase
    {
    }
}
