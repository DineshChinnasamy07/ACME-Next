/*  Class Name      : RightsController.cs
 *  Purpose         : User Rights endpoints (GetUsersType, GetAllUserRights, UpdateUserRights)
 *  Author          : Dinesh C
 *  Created on      : 27-Oct-2026
 */
using Bosco.Auth.Application.Dtos;
using Bosco.Auth.Application.Features.UserManagement;
using Bosco.Auth.Infrastructure.Security;
using Bosco.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bosco.Auth.Api.Controllers
{
    // Portal admin works on the portal database; a head office login works on its own head office database
    // (HEAD_OFFICE_CODE claim in the token).
    [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
    [ApiExplorerSettings(GroupName = "Rights")]
    [ApiController]
    [Route("api/rights")]
    public class RightsController : ControllerBase
    {
        private readonly RightSystem _rights;

        public RightsController(RightSystem rights)
        {
            _rights = rights;
        }

        private string ReadHeadOfficeCode()
        {
            return User.FindFirstValue(JwtTokenService.HeadOfficeCodeClaim) ?? string.Empty;
        }

        /// <summary>
        /// Lists roles used for the rights screen.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The role list for rights.</returns>
        [HttpGet("roles")]
        public async Task<IActionResult> GetUsersType(CancellationToken cancellationToken)
        {
            return Ok(await _rights.GetUsersType(ReadHeadOfficeCode(), cancellationToken));
        }

        /// <summary>
        /// Loads all rights for the selected role.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The rights for the role.</returns>
        [HttpGet("{roleId:int}")]
        public async Task<IActionResult> GetAllUserRights(int roleId, CancellationToken cancellationToken)
        {
            return Ok(await _rights.GetAllUserRights(roleId, ReadHeadOfficeCode(), cancellationToken));
        }

        /// <summary>
        /// Updates the rights assigned to a role.
        /// </summary>
        /// <param name="request">The role and rights to save.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The update result.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateUserRights([FromBody] UpdateUserRightsRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Role is required." });
            }

            RightsActionResult result = await _rights.UpdateUserRights(request, ReadHeadOfficeCode(), cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }
    }
}
