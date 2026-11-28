/*  Class Name      : MapBranchMastersController.cs
 *  Purpose         : Generic Head Office -> Branch master mapping. Head office maps master records
 *                    (ledger, legal entity, project category, ledger group, purpose) to a branch; a
 *                    branch login reads the records mapped to it. Route: api/mapbranchmasters.
 *  Author          : Dinesh C
 *  Created on      : 27-Nov-2026
 */
using Acme.Common;
using Acme.Common.Security;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.Mapping.MapBranchMasters;
using Acme.Masters.Application.Features.HeadOfficeMasters.Mapping.MapBranchMasters;
using Acme.Masters.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Masters.Api.MastersControllers.HeadOfficeMastersController.Mapping
{
    // Reads for a branch login are allowed (the branch masters list); the mapping admin actions below
    // re-assert PortalOrHeadOfficePolicy so only head office can change the mappings.
    [Authorize(Policy = JwtAuthenticationExtensions.PortalHeadOfficeOrBranchPolicy)]
    [ApiExplorerSettings(GroupName = "HeadOfficeMasters")]
    [ApiController]
    [Route("api/mapbranchmasters")]
    public class MapBranchMastersController : ControllerBase
    {
        private readonly BranchMasterMappingSystem _mapping;

        public MapBranchMastersController(BranchMasterMappingSystem mapping)
        {
            _mapping = mapping;
        }

        // ---- Head office mapping screen ----

        [HttpGet("branches")]
        [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
        public async Task<IActionResult> FetchBranches(CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _mapping.FetchBranches(office, cancellationToken));
        }

        [HttpGet("{masterType}/records")]
        [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
        public async Task<IActionResult> FetchRecords(string masterType, CancellationToken cancellationToken)
        {
            if (!BranchMasterMappingSystem.IsSupportedType(masterType))
            {
                return BadRequest(new { message = "This master type cannot be mapped yet." });
            }

            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _mapping.FetchRecords(office, masterType, cancellationToken));
        }

        [HttpGet("{masterType}/mapped")]
        [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
        public async Task<IActionResult> FetchMappedIds(string masterType, [FromQuery] int branchId, CancellationToken cancellationToken)
        {
            if (!BranchMasterMappingSystem.IsSupportedType(masterType))
            {
                return BadRequest(new { message = "This master type cannot be mapped yet." });
            }

            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            return Ok(await _mapping.FetchMappedIds(office, masterType, branchId, cancellationToken));
        }

        [HttpPost]
        [Authorize(Policy = JwtAuthenticationExtensions.PortalOrHeadOfficePolicy)]
        public async Task<IActionResult> SaveMapping([FromBody] BranchMasterMappingSaveRequest? request, CancellationToken cancellationToken)
        {
            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;
            BranchMasterMappingResult result = await _mapping.SaveMapping(office, request, cancellationToken);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        // ---- Branch login read ----

        [HttpGet("branch/{masterType}")]
        public async Task<IActionResult> FetchBranchMasters(string masterType, CancellationToken cancellationToken)
        {
            if (!BranchMasterMappingSystem.IsSupportedType(masterType))
            {
                return BadRequest(new { message = "This master type is not available." });
            }

            (HeadOfficeConnection? office, IActionResult? error) = await ResolveOffice(cancellationToken);
            if (office == null) return error!;

            string? branchScope = User.FindBranchScope();
            if (string.IsNullOrWhiteSpace(branchScope))
            {
                // Head office / portal user hitting the branch route: nothing branch-specific to return.
                return Ok(Array.Empty<BranchMasterRecordDto>());
            }

            return Ok(await _mapping.FetchBranchMasters(office, branchScope, masterType, cancellationToken));
        }

        private async Task<(HeadOfficeConnection? Office, IActionResult? Error)> ResolveOffice(CancellationToken cancellationToken)
        {
            string? scope = User.FindHeadOfficeScope();
            if (scope == null)
            {
                return (null, StatusCode(StatusCodes.Status403Forbidden, new { message = "This action is available to head office and branch logins only." }));
            }

            HeadOfficeConnection? office = await _mapping.ResolveOffice(scope, cancellationToken);
            if (office == null)
            {
                return (null, NotFound(new { message = MessageRender.GetMessage(MessageCatalog.Office.HeadOfficeNotFound) }));
            }

            return (office, null);
        }
    }
}
