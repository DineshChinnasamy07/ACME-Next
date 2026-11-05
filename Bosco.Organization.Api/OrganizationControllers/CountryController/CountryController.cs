/*  Class Name      : CountryController.cs
 *  Purpose         : Country and state dropdown endpoints for Office
 *  Author          : Dinesh C
 *  Created on      : 28-Oct-2026
 */
using Bosco.Organization.Application.Features.Office;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bosco.Organization.Api.HeadOfficeControllers
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "Country")]
    [ApiController]
    [Route("api/countries")]
    public class CountryController : ControllerBase
    {
        private readonly HeadOfficeSystem _headOffices;

        public CountryController(HeadOfficeSystem headOffices)
        {
            _headOffices = headOffices;
        }

        /// <summary>
        /// Lists countries for the Country dropdown.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The country list.</returns>
        [HttpGet]
        public async Task<IActionResult> FetchCountry(CancellationToken cancellationToken)
        {
            return Ok(await _headOffices.FetchCountry(cancellationToken));
        }
        /// <summary>
        /// Lists states for the State dropdown by country.
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{countryId:int}/states")]
        public async Task<IActionResult> FetchStateByCountry(int countryId, CancellationToken cancellationToken) 
        {
            return Ok(await _headOffices.FetchStateByCountry(countryId, cancellationToken));
        }
    }
}
