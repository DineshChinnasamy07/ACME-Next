/*  Class Name      : MapProjectsController.cs
 *  Purpose         : Head Office Mapping - MapProjects endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Masters.Api.MastersControllers.HeadOfficeMastersController.Mapping
{
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/mapprojects")]
    [Authorize]
    public class MapProjectsController : ControllerBase
    {
    }
}
