/*  Class Name      : RolesController.cs
 *  Purpose         : User Role endpoints (FetchUserRoleDetails, SaveUserRoles, DeleteUserRole)
 *  Author          : Dinesh C
 *  Created on      : 22-Oct-2026
 */
using Bosco.Auth.Application.Dtos;
using Bosco.Auth.Application.Features.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Auth.Api.Controllers
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "Roles")]
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly UserRoleSystem _roles;

        public RolesController(UserRoleSystem roles)
        {
            _roles = roles;
        }

        /// <summary>
        /// Lists all user roles.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The user role list.</returns>
        [HttpGet]
        public async Task<IActionResult> FetchUserRoleDetails(CancellationToken cancellationToken)
        {
            return Ok(await _roles.FetchUserRoleDetails(cancellationToken));
        }

        /// <summary>
        /// Loads one user role by id for edit or view.
        /// </summary>
        /// <param name="userRoleId">The user role identifier.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The user role details when found; otherwise not found.</returns>
        [HttpGet("{userRoleId:int}")]
        public async Task<IActionResult> FillSaveUserRole(int userRoleId, CancellationToken cancellationToken)
        {
            RoleSaveRequest? row = await _roles.FillSaveUserRole(userRoleId, cancellationToken);
            if (row == null)
            {
                return NotFound(new { message = "User Role not found." });
            }

            return Ok(row);
        }

        /// <summary>
        /// Adds or updates a user role.
        /// </summary>
        /// <param name="request">The role details to save.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The save result.</returns>
        [HttpPost]
        public async Task<IActionResult> SaveUserRoles([FromBody] RoleSaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Role Name is required" });
            }

            RoleActionResult result = await _roles.SaveUserRoles(request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// Deletes a user role by id.
        /// </summary>
        /// <param name="userRoleId">The user role identifier.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The delete result.</returns>
        [HttpDelete("{userRoleId:int}")]
        public async Task<IActionResult> DeleteUserRole(int userRoleId, CancellationToken cancellationToken)
        {
            RoleActionResult result = await _roles.DeleteUserRole(userRoleId, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }
    }
}
