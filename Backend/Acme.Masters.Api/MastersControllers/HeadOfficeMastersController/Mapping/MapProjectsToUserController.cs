/*  Class Name      : MapProjectsToUserController.cs
 *  Purpose         : Head Office Mapping - MapProjectsToUser endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.Mapping
{
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/mapprojectstouser")]
    [Authorize]
    public class MapProjectsToUserController : ControllerBase
    {
    }
}
