/*  Class Name      : MapBranchToUserController.cs
 *  Purpose         : Head Office Mapping - MapBranchToUser endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.Mapping
{
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/mapbranchtouser")]
    [Authorize]
    public class MapBranchToUserController : ControllerBase
    {
    }
}
