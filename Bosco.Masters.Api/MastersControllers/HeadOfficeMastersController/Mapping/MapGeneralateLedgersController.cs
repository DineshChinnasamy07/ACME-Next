/*  Class Name      : MapGeneralateLedgersController.cs
 *  Purpose         : Head Office Mapping - MapGeneralateLedgers endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Masters.Api.MastersControllers.HeadOfficeMastersController.Mapping
{
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/mapgeneralateledgers")]
    [Authorize]
    public class MapGeneralateLedgersController : ControllerBase
    {
    }
}
