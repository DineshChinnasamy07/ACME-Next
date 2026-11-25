/*  Class Name      : MapSubsidyLedgersController.cs
 *  Purpose         : Head Office Mapping - MapSubsidyLedgers endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.Mapping
{
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/mapsubsidyledgers")]
    [Authorize]
    public class MapSubsidyLedgersController : ControllerBase
    {
    }
}
