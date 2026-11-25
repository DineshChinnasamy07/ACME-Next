/*  Class Name      : MasterLedgerConfiguration.cs
 *  Purpose         : Map MasterLedger to public.master_ledger
 *  Author          : Dinesh C
 *  Created on      : 15-Nov-2026
 */
using Acme.Masters.Domain.Entities.HeadOfficeMasters.FinanceMasters.LedgerGroup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Acme.Masters.Infrastructure.Persistence.Configurations.HeadOfficeMasters.FinanceMasters.LedgerGroup
{
    public class MasterLedgerConfiguration : IEntityTypeConfiguration<MasterLedger>
    {
        public void Configure(EntityTypeBuilder<MasterLedger> builder)
        {
            builder.ToTable("master_ledger");
            builder.HasKey(row => row.LedgerId);

            builder.Property(row => row.LedgerId).HasColumnName("LEDGER_ID").ValueGeneratedOnAdd();
            builder.Property(row => row.LedgerCode).HasColumnName("LEDGER_CODE").HasMaxLength(15);
            builder.Property(row => row.LedgerName).HasColumnName("LEDGER_NAME").HasMaxLength(100);
            builder.Property(row => row.GroupId).HasColumnName("GROUP_ID");
            builder.Property(row => row.LedgerType).HasColumnName("LEDGER_TYPE").HasMaxLength(3);
            builder.Property(row => row.LedgerSubType).HasColumnName("LEDGER_SUB_TYPE").HasMaxLength(3);
            builder.Property(row => row.FdInvestmentTypeId).HasColumnName("FD_INVESTMENT_TYPE_ID");
            builder.Property(row => row.BankAccountId).HasColumnName("BANK_ACCOUNT_ID");
            builder.Property(row => row.IsCostCenter).HasColumnName("IS_COST_CENTER");
            builder.Property(row => row.IsTdsLedger).HasColumnName("IS_TDS_LEDGER");
            builder.Property(row => row.Notes).HasColumnName("NOTES").HasMaxLength(500);
            builder.Property(row => row.IsBankInterestLedger).HasColumnName("IS_BANK_INTEREST_LEDGER");
            builder.Property(row => row.IsBankFdPenaltyLedger).HasColumnName("IS_BANK_FD_PENALTY_LEDGER");
            builder.Property(row => row.IsBankSbInterestLedger).HasColumnName("IS_BANK_SB_INTEREST_LEDGER");
            builder.Property(row => row.IsBankCommissionLedger).HasColumnName("IS_BANK_COMMISSION_LEDGER");
            builder.Property(row => row.SortId).HasColumnName("SORT_ID");
            builder.Property(row => row.Status).HasColumnName("STATUS");
            builder.Property(row => row.AccessFlag).HasColumnName("ACCESS_FLAG");
            builder.Property(row => row.DateClosed).HasColumnName("DATE_CLOSED");
            builder.Property(row => row.ClosedBy).HasColumnName("CLOSED_BY");
            builder.Property(row => row.IsBranchLedger).HasColumnName("IS_BRANCH_LEDGER");
            builder.Property(row => row.IsInkindLedger).HasColumnName("IS_INKIND_LEDGER");
            builder.Property(row => row.IsDepreciationLedger).HasColumnName("IS_DEPRECIATION_LEDGER");
            builder.Property(row => row.IsAssetGainLedger).HasColumnName("IS_ASSET_GAIN_LEDGER");
            builder.Property(row => row.IsAssetLossLedger).HasColumnName("IS_ASSET_LOSS_LEDGER");
            builder.Property(row => row.IsDisposalLedger).HasColumnName("IS_DISPOSAL_LEDGER");
            builder.Property(row => row.BudgetGroupId).HasColumnName("BUDGET_GROUP_ID");
            builder.Property(row => row.BudgetSubGroupId).HasColumnName("BUDGET_SUB_GROUP_ID");
        }
    }
}
