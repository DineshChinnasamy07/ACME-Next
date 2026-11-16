/*  Class Name      : MastersServiceCollectionExtensions.cs
 *  Purpose         : Register Masters EF DbContext and services
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.Ledger;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.Project;
using Bosco.Masters.Application.Abstractions.Repositories.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.Ledger;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.Project;
using Bosco.Masters.Application.Features.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Bosco.Masters.Infrastructure.Persistence;
using Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.AccountingPeriod;
using Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.FcPurpose;
using Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.GeneralateLedgerGroup;
using Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.Ledger;
using Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.LockVoucher;
using Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.Project;
using Bosco.Masters.Infrastructure.Repositories.HeadOfficeMasters.FinanceMasters.ProjectCategory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bosco.Masters.Infrastructure.Extensions
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

            return services;
        }
    }
}
