/*  Class Name      : MapGoverningBodiesController.cs
 *  Purpose         : Head Office Statutory Masters - MapGoverningBodies endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Masters.Api.MastersControllers.HeadOfficeMastersController.StatutoryMasters
{
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/mapgoverningbodies")]
    [Authorize]
    public class MapGoverningBodiesController : ControllerBase
    {
    }
}
