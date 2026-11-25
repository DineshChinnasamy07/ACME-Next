/*  Class Name      : MastersServiceCollectionExtensions.cs
 *  Purpose         : Register Masters EF DbContext and services
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.Ledger;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.Project;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.Ledger;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.Project;
using Acme.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Acme.Masters.Infrastructure.Persistence;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.Ledger;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.Project;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.TdsSection;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.NatureOfPayments;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.DeducteeType;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.DutyTax;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.TdsPolicy;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.LegalEntity;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.GoverningBodies;
using Acme.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Application.Features.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Acme.Masters.Infrastructure.Repositories.HeadOfficeMasters.StatutoryMasters.MapGoverningBodies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.Masters.Infrastructure.Extensions
{
    public static class MastersServiceCollectionExtensions
    {
        public const string ConnectionName = "AcpErp";

        public static IServiceCollection AddMastersServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString(ConnectionName) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'AcpErp' is missing. Set ConnectionStrings:AcpErp in appsettings.json.");
            }

            services.AddDbContext<MastersDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
            services.AddScoped<AccountingPeriodSystem>();
            services.AddScoped<ILedgerGroupRepository, LedgerGroupRepository>();
            services.AddScoped<LedgerGroupSystem>();
            services.AddScoped<ILedgerRepository, LedgerRepository>();
            services.AddScoped<LedgerSystem>();
            services.AddScoped<IProjectCategoryRepository, ProjectCategoryRepository>();
            services.AddScoped<ProjectCatogorySystem>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ProjectSystem>();
            services.AddScoped<IFcPurposeRepository, FcPurposeRepository>();
            services.AddScoped<PurposeSystem>();
            services.AddScoped<IGeneralateLedgerGroupRepository, GeneralateLedgerGroupRepository>();
            services.AddScoped<GeneralateSystem>();
            services.AddScoped<ILockVoucherRepository, LockVoucherRepository>();
            services.AddScoped<LockVoucherSystem>();
            services.AddScoped<ITdsSectionRepository, TdsSectionRepository>();
            services.AddScoped<TdsSectionSystem>();
            services.AddScoped<INatureOfPaymentRepository, NatureOfPaymentRepository>();
            services.AddScoped<NatureOfPaymentSystem>();
            services.AddScoped<IDeducteeTypeRepository, DeducteeTypeRepository>();
            services.AddScoped<DeducteeTypeSystem>();
            services.AddScoped<IDutyTaxRepository, DutyTaxRepository>();
            services.AddScoped<DutyTaxSystem>();
            services.AddScoped<ITdsPolicyRepository, TdsPolicyRepository>();
            services.AddScoped<TdsPolicySystem>();
            services.AddScoped<ILegalEntityRepository, LegalEntityRepository>();
            services.AddScoped<LegalEntitySystem>();
            services.AddScoped<IGoverningBodiesRepository, GoverningBodiesRepository>();
            services.AddScoped<ExecutiveMemberSystem>();
            services.AddScoped<IMapGoverningBodiesRepository, MapGoverningBodiesRepository>();
            services.AddScoped<GoverningMemberMappingSystem>();

            return services;
        }
    }
}
