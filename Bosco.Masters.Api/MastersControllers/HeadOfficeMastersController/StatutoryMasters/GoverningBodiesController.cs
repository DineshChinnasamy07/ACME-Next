/*  Class Name      : GoverningBodiesController.cs
 *  Purpose         : Head Office Statutory Masters - GoverningBodies endpoints
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Masters.Api.MastersControllers.HeadOfficeMastersController.StatutoryMasters
{
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/governingbodies")]
    [Authorize]
    public class GoverningBodiesController : ControllerBase
    {
    }
}
