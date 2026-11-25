/*  Class Name      : LegalEntitySystem.cs
 *  Purpose         : Legal Entity business entry point (legacy Bosco.Model LegalEntitySystem)
 *  Author          : Dinesh C
 *  Created on      : 24-Nov-2026
 */
using Acme.Common;
using Acme.Common.Messages;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Application.Dtos.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Domain.Entities;

namespace Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.LegalEntity
{
    public class LegalEntitySystem
    {
        private readonly ILegalEntityRepository _legalEntities;

        public LegalEntitySystem(ILegalEntityRepository legalEntities)
        {
            _legalEntities = legalEntities;
        }

        public Task<HeadOfficeConnection?> ResolveOffice(string headOfficeCode, CancellationToken cancellationToken)
        {
            return _legalEntities.ResolveOffice(headOfficeCode, cancellationToken);
        }

        public Task<IReadOnlyList<LegalEntityListItemDto>> FetchLegalEntity(HeadOfficeConnection office, CancellationToken cancellationToken)
        {
            return _legalEntities.FetchLegalEntity(office, cancellationToken);
        }

        /// <summary>
        /// Static option lists of the legacy LegalEntityAdd page (rdAssociationNature / rdDenomination).
        /// </summary>
        public LegalEntityLookupsDto LoadLegalEntityLookups()
        {
            return new LegalEntityLookupsDto
            {
                AssociationNatures = new List<LegalEntityOptionDto>
                {
                    new LegalEntityOptionDto { Id = 0, Name = "Cultural" },
                    new LegalEntityOptionDto { Id = 1, Name = "Economic" },
                    new LegalEntityOptionDto { Id = 2, Name = "Educational" },
                    new LegalEntityOptionDto { Id = 3, Name = "Religious" },
                    new LegalEntityOptionDto { Id = 4, Name = "Social" },
                    new LegalEntityOptionDto { Id = 5, Name = "Others" }
                },
                Denominations = new List<LegalEntityOptionDto>
                {
                    new LegalEntityOptionDto { Id = 1, Name = "Hindu" },
                    new LegalEntityOptionDto { Id = 2, Name = "Sikh" },
                    new LegalEntityOptionDto { Id = 3, Name = "Muslim" },
                    new LegalEntityOptionDto { Id = 4, Name = "Christian" },
                    new LegalEntityOptionDto { Id = 5, Name = "Buddhist" },
                    new LegalEntityOptionDto { Id = 6, Name = "Others" }
                }
            };
        }

        public Task<LegalEntityDetailDto?> FillLegalEntityDetails(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken)
        {
            if (customerId <= 0)
            {
                return Task.FromResult<LegalEntityDetailDto?>(null);
            }

            return _legalEntities.FillLegalEntityDetails(office, customerId, cancellationToken);
        }

        public Task<LegalEntityActionResult> SaveLegalEntityDetails(HeadOfficeConnection office, LegalEntitySaveRequest? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return Task.FromResult(new LegalEntityActionResult
                {
                    Success = false,
                    Message = MessageRender.GetMessage(GoverningLegalMessages.LEGAL_ENTITY_DETAILS_REQUIRED)
                });
            }

            return _legalEntities.SaveLegalEntityDetails(office, request, cancellationToken);
        }

        public Task<LegalEntityActionResult> DeleteLegalEntityDetails(HeadOfficeConnection office, int customerId, CancellationToken cancellationToken)
        {
            return _legalEntities.DeleteLegalEntityDetails(office, customerId, cancellationToken);
        }
    }
}
