/*  Class Name      : PermissionsController.cs
 *  Purpose         : Auth service file
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Bosco.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Auth.Api.Controllers
{
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOnlyPolicy)]
    [ApiExplorerSettings(GroupName = "Permissions")]
    [ApiController]
    [Route("api/permissions")]
    public class PermissionsController : ControllerBase
    {
    }
}
