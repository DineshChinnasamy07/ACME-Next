// Top-bar menus, ported from the legacy acme.erp sitemaps:
//   siteAdminMenu  ← ACCPPORTAL/AcMeERP/SiteMap/siteadminmenu.sitemap  (portal login)
//   headOfficeMenu ← ACCPPORTAL/AcMeERP/SiteMap/headofficemenu.sitemap (head office login)
// Items with an `id` that matches a screen in AppShell open that screen; the rest show their title.
// `module` / `activity` keep the legacy rights codes so menus can later be filtered by user rights.

function leaf(title, module, activity, id) {
    return { id: id || `${module}.${activity}`.toLowerCase(), title, module, activity };
}

function group(title, module, activity, children) {
    return { id: `group.${module}.${activity}.${title}`.toLowerCase(), title, module, activity, children };
}

const home = { id: 'home', title: 'Home' };

const userMenu = (title) => group(title, 'User', 'UserView', [
    leaf('User', 'User', 'UserView', 'user-list'),
    leaf('User Role', 'User', 'UserRoleView', 'user-role'),
    leaf('User Rights', 'User', 'UserRights', 'user-rights'),
    leaf('Change Password', '', '', 'change-password'),
]);

export const siteAdminMenu = [
    home,
    userMenu('User'),
    group('Office', 'Office', 'HeadOfficeView', [
        leaf('Head Office', 'Office', 'HeadOfficeView', 'head-office'),
        leaf('Branch Office', 'Office', 'BranchOfficeView', 'branch-office'),
    ]),
    group('Tools', 'Tools', 'SoftwareUpload', [
        leaf('Upload Product', 'Tools', 'SoftwareUpload'),
        leaf('Download Key', 'Tools', 'DownloadLicenseKey'),
        leaf('Acme.erp Template', 'Tools', 'DownloadTemplate'),
        leaf('Upload Voucher (Off-line) and Status', 'Tools', 'UploadVoucherFile'),
        leaf('Delete Voucher', 'Tools', 'DeleteVoucher'),
        leaf('Execute Query', 'Tools', 'ExcuteQuery'),
        leaf('Clear Log', 'Tools', 'ClearLogData'),
        leaf('Import Branches', 'Tools', 'ImportBranches'),
        leaf('Acme Service', 'Tools', 'AcmeService'),
        leaf('Branch Logged History', 'Tools', 'BranchLoggedHistory'),
        leaf('Branch Location Logged History', 'Tools', 'BranchLocationLoggedHistory'),
        leaf('HO wise Branch List', 'Tools', 'HOwiseBranchList'),
        leaf('HO wise Branch Details', 'Tools', 'BranchListInfo'),
        leaf('HO wise Branch Count', 'Tools', 'BranchListCount'),
        leaf('Country', 'Data', 'CountryView'),
        leaf('State', 'Data', 'StateView'),
    ]),
    group('Ticketing', 'TroubleTicket', 'TrackTicket', [
        leaf('Reply Ticket', 'TroubleTicket', 'TrackTicket'),
    ]),
    group('Help', 'Help', 'Help', [
        leaf('Head Office Manual', 'UserManual', 'HOUserManual'),
        leaf('Branch Office Manual', 'UserManual', 'BOUserManual'),
    ]),
];

export const headOfficeMenu = [
    home,
    userMenu('Users'),
    group('Office', 'Office', 'BranchOfficeView', [
        leaf('Branch Office', 'Office', 'BranchOfficeView', 'branch-office'),
        leaf('Branch Location', 'Office', 'BranchLocationView'),
        leaf('Refresh Ledger Balance', 'Office', 'RefreshBalance'),
    ]),
    group('Data', 'Data', 'LegalEntityView', [
        group('Finance Masters', 'Data', 'LegalEntityView', [
            leaf('Accounting Period', 'Data', 'AccountingPeriod'),
            leaf('Ledger Group', 'Data', 'LedgerGroupView'),
            leaf('Ledger', 'Data', 'LedgerView'),
            leaf('Project Category', 'Data', 'ProjectCategoryView'),
            leaf('Project', 'Data', 'ProjectView'),
            leaf('FC Purpose', 'Data', 'FCPurposeView'),
            leaf('Generalate Ledger Group', 'Data', 'GeneralateLedgerGroup'),
            leaf('Lock Vouchers', 'Data', 'LockVoucherView'),
        ]),
        group('Statutary Masters', 'Data', 'GoverningMemberView', [
            leaf('Legal Entity', 'Data', 'LegalEntityView'),
            leaf('Governing Bodies', 'Data', 'GoverningMemberView'),
            leaf('Map Governing Bodies', 'Data', 'MapGoverningBody'),
            leaf('TDS Section', 'Data', 'TDSSectionView'),
            leaf('Nature of Payments', 'Data', 'TDSPaymentNatureView'),
            leaf('Deductee Type', 'Data', 'TDSDeducteeTypeView'),
            leaf('TDS Policy', 'Data', 'TDSPolicyView'),
            leaf('Duty Tax', 'Data', 'TDSDutyTaxView'),
        ]),
        group('Mapping', 'Data', 'GoverningMemberView', [
            leaf('Project Mapping', 'Data', 'ProjectMapping'),
            leaf('Branch Master Mapping', 'Data', 'BranchMasterMapping', 'data.branchmastermapping'),
            leaf('Map Ledgers', 'Data', 'MapLedgerstoBranch'),
            leaf('Map Branch to User', 'Data', 'MapBranchToUser'),
            leaf('Map Subsidy Ledgers', 'Data', 'MapSubsidyLedgers'),
            leaf('Map Generalate Ledgers', 'Data', 'MAPGENERALATELEDGERS'),
            leaf('Map Generalate Ledgers by Project Category Group', 'Data', 'PCGGeneralateLedgerMapping'),
            leaf('Generalate Opening Balance', 'Data', 'GENERALATEOPENING'),
        ]),
        group('Vouchers', 'Data', 'VoucherView', [
            leaf('Vouchers', 'Data', 'VoucherView'),
            leaf('Fixed Deposit', 'Data', 'FDRegistersView'),
            leaf('Amendments', 'Data', 'AmendmentView'),
        ]),
        group('Budget', 'Data', 'Budget', [
            leaf('Approve Budget', 'Data', 'ApproveBudget'),
            leaf('Approve Budget (Mysore)', 'Data', 'MysoreApproveBudget'),
        ]),
        group('Settings', 'Data', 'SETTINGS', [
            leaf('Global Setting', 'Data', 'SETTINGS'),
            leaf('Disable/Approve Branch Receipt Module', 'Data', 'RequestReceiptModule'),
        ]),
    ]),
    group('Budget', 'BudgetINM', 'BudgetINM', [
        leaf('Budget Approved', 'BudgetINM', 'BudgetINM'),
    ]),
    group('Monitor', 'Monitor', 'Monitor', [
        leaf('Project Details', 'ProjectDetails', 'ProjectDetails'),
    ]),
    group('Reports', 'ReportAbstract', 'MonthlyAbstract', [
        group('Masters', 'ReportMasters', 'HeadOfficeLedgers', [
            leaf('Head Office Ledgers', 'ReportMasters', 'HeadOfficeLedgers', 'rpt-052'),
            leaf('Head Office Project', 'ReportMasters', 'HeadOfficeProject', 'rpt-186'),
            leaf('Branch Bank Accounts', 'ReportMasters', 'BranchBankList', 'rpt-059'),
            leaf('Branch-wise Balance', 'ReportMasters', 'BranchwiseBalance', 'rpt-060'),
            leaf('Branch Data Status', 'ReportMasters', 'BranchDataStatus', 'rpt-061'),
            leaf('Branch Recent Export Details', 'ReportMasters', 'Branch Export Voucher', 'rpt-067'),
            leaf('Details of all Institution (Subsidy/Contribution)', 'ReportMasters', 'HeadOfficeLedgers', 'rpt-053'),
            leaf('Foreign Institution Details (Subsidy/Contribution)', 'ReportMasters', 'ForeignInstitutionDetails', 'rpt-054'),
            leaf('Categorywise Institution Details (Subsidy/Contribution)', 'ReportMasters', 'CategoryWiseInstitutionDetails', 'rpt-055'),
        ]),
        group('Abstract', 'ReportAbstract', 'MonthlyAbstract', [
            leaf('Monthly - Receipts', 'ReportAbstract', 'MonthlyReceipt', 'rpt-001'),
            leaf('Monthly - Payments', 'ReportAbstract', 'MonthlyPayment', 'rpt-002'),
            leaf('Monthly - Receipts and Payments', 'ReportAbstract', 'MonthlyReceiptPayment', 'rpt-003'),
            leaf('Multi - Receipts', 'ReportAbstract', 'MultiabstractReceipt', 'rpt-004'),
            leaf('Multi - Payments', 'ReportAbstract', 'MultiabstractPayment', 'rpt-005'),
            leaf('Multi - Receipts and Payments', 'ReportAbstract', 'MultiabstractPaymentReceipt', 'rpt-006'),
        ]),
        group('Bank Activities', 'ReportBankActivities', 'BankBalanceStatement', [
            leaf('Bank Balance Statement', 'ReportBankActivities', 'BankBalanceStatement', 'rpt-014'),
            leaf('Fixed Deposit Statement', 'ReportBankActivities', 'FixedDepositStatement', 'rpt-015'),
        ]),
        group('Books of Accounts', 'ReportBookofAccounts', 'CashandBankBook', [
            leaf('Cash Journal', 'ReportBookofAccounts', 'CashJournal', 'rpt-016'),
            leaf('Bank Journal', 'ReportBookofAccounts', 'BankJournal', 'rpt-017'),
            leaf('Cash and Bank Book', 'ReportBookofAccounts', 'CashandBankBook', 'rpt-011'),
            leaf('Ledger', 'ReportBookofAccounts', 'Ledger', 'rpt-012'),
            leaf('Cash Daily Balance', 'ReportBookofAccounts', 'CashDailyBalance', 'rpt-018'),
            leaf('Bank Daily Balance', 'ReportBookofAccounts', 'BankDailyBalance', 'rpt-019'),
            leaf('Financial Position', 'ReportBookofAccounts', 'FinancialPosition', 'rpt-066'),
        ]),
        group('Final Accounts', 'ReportFinalAccounts', 'AccountsTrialBalance', [
            leaf('Trial Balance', 'ReportFinalAccounts', 'AccountsTrialBalance', 'rpt-030'),
            leaf('Receipts and Payments', 'ReportFinalAccounts', 'ACCOUNTSRECEIPTSANDPAYMENTS', 'rpt-027'),
            leaf('Receipts and Payments (SAP)', 'ReportFinalAccounts', 'SAPACCOUNTSRECEIPTSANDPAYMENTS', 'rpt-183'),
            leaf('Income and Expenditure', 'ReportFinalAccounts', 'AccountsIncomeandExpenditure', 'rpt-028'),
            leaf('Balance Sheet', 'ReportFinalAccounts', 'ACCOUNTSBALANCESHEET', 'rpt-031'),
            leaf('Balance Sheet (SAP)', 'ReportFinalAccounts', 'SAPACCOUNTSBALANCESHEET', 'rpt-184'),
            leaf('Profit and Loss Branch-wise', 'ReportFinalAccounts', 'AccountsPL', 'rpt-182'),
            leaf('Province Percentage', 'ReportFinalAccounts', 'ProvincePercentageReports', 'rpt-197'),
        ]),
        group('Branch Analysis', 'ReportFinalAccounts', 'AccountsTrialBalance', [
            leaf('Branch-Wise Income and Expense (Month-Wise)', 'ReportFinalAccounts', 'CommunityIncomeExpenseMonthWise', 'rpt-050'),
            leaf('Branch-Wise Income and Expense (Year-Wise)', 'ReportFinalAccounts', 'CommunityIncomeExpenseYearWise', 'rpt-188'),
            leaf('Branch Location-Wise Income and Expense (Year-Wise)', 'ReportFinalAccounts', 'CommunityLocationIncomeExpenseYearWise', 'rpt-189'),
            leaf('Branch and Category-Wise Ledger', 'ReportFinalAccounts', 'CollectiveLedgerReport', 'rpt-051'),
            leaf('Branch-Wise Fixed Asset Investments (Year-Wise)', 'ReportFinalAccounts', 'BranchWiseInvestments', 'rpt-187'),
            leaf('Branch-Wise Monthly Ledger Summary', 'ReportFinalAccounts', 'MonthWiseLedgerReport', 'rpt-058'),
        ]),
        group('Foreign Contribution', 'ReportForeignContribution', 'FCPurpose', [
            leaf('FC Purpose', 'ReportForeignContribution', 'FCPurpose', 'rpt-020'),
            leaf('FC Purpose Donor Institutional', 'ReportForeignContribution', 'FCPurposeDonorInstitutional', 'rpt-021'),
            leaf('FC Purpose Donor Individual', 'ReportForeignContribution', 'FCPurposeDonorIndividual', 'rpt-022'),
            leaf('FC Country', 'ReportForeignContribution', 'FCCountry', 'rpt-042'),
        ]),
        group('Budget', 'FinalBudgetData', 'BudgetDetails', [
            leaf('Budget Details', 'FinalBudgetData', 'BudgetDetails', 'rpt-048'),
            leaf('Budget Variance', 'FinalBudgetData', 'BudgetVariance', 'rpt-046'),
            leaf('Mysore Budget', 'FinalBudgetData', 'BudgetApprovalByMonth', 'rpt-171'),
            leaf('Budget Proposed and Approved Details', 'FinalBudgetData', 'BudgetView', 'rpt-180'),
        ]),
        group('Generalate Reports', 'REPORTGENERALATE', 'REPORTGENERALATE', [
            group('Rendiconto Report', 'Rendiconto', 'REPORTGENERALATE1', [
                leaf('Profit and Loss - Summary', 'REPORTGENERALATE', 'LegalEntitywiseConsolidatedIE', 'rpt-062'),
                leaf('Profit and Loss - Detailed', 'REPORTGENERALATE', 'LegalEntitywiseIE', 'rpt-063'),
                leaf('Balance Sheet', 'REPORTGENERALATE', 'BalanceSheetGeneralate', 'rpt-065'),
            ]),
            group('Rendiconto Report(Verification)', 'REPORTGENERALATE', 'REPORTGENERALATE2', [
                leaf('Receipts and Payments', 'REPORTGENERALATE', 'ReportGeneralateRP', 'rpt-079'),
                leaf('Profit and Loss - Summary', 'REPORTGENERALATE', 'ReportGeneralateInterAccVerification', 'rpt-068'),
                leaf('Profit and Loss - Detailed', 'REPORTGENERALATE', 'ReportGeneralatePLVerification', 'rpt-078'),
                leaf('Profit and Loss - Branch Summary', 'REPORTGENERALATE', 'ReportGeneralateInterAccVerificationByBranch', 'rpt-190'),
                leaf('Balance Sheet', 'REPORTGENERALATE', 'ReportGeneralateBalanceSheetVerification', 'rpt-077'),
            ]),
            leaf('Generalate Ledger Balance', 'GeneralateLedgerBalance', 'GeneralateLedgerBalance', 'rpt-178'),
            leaf('Community Reports', 'CommunityGeneralateReport', 'CommunityGeneralateReport'),
        ]),
        group('Annual Reports', 'GeneralateAnnualReports', 'GeneralateAnnualReports', [
            leaf('Annual Report - Community', 'GeneralateAnnualReports', 'ActivityBasedFDCCSICommunityAnnualReport', 'rpt-170'),
            leaf('Annual Report - Apostolic Activities', 'GeneralateAnnualReports', 'ActivityBasedFDCCSIApostolicAnnualReport', 'rpt-179'),
            leaf('Budget - Community', 'GeneralateAnnualReports', 'GeneralateAnnualReportsApostolicBudget', 'rpt-175'),
            leaf('Budget - Apostolic Activities', 'GeneralateAnnualReports', 'GeneralateAnnualReportsCommunityBudget', 'rpt-176'),
        ]),
    ]),
    group('Tools', 'Tools', 'SoftwareDownload', [
        leaf('Download Product', 'Tools', 'SoftwareDownload'),
        leaf('Download Key', 'Tools', 'DownloadLicenseKey'),
        leaf('Download Master', 'Tools', 'DownloadMasterData'),
        leaf('Download Template', 'Tools', 'DownloadTemplate'),
        leaf('Upload Voucher (Off-line) and Status', 'Tools', 'UploadVoucherFile'),
        leaf('Message', 'Tools', 'SendMessage'),
        leaf('Acme.erp Backup', 'Tools', 'acmeerpBackup'),
        leaf('Acme.erp Backup 2015-23', 'Tools', 'ACMEERPBACKUPOLD'),
        leaf('Acme.erp Backup-Ms Access', 'Tools', 'ACMEERPBACKUPMS'),
        leaf('Acme.erp Branch Reports', 'Tools', 'acmeerpbranchreports'),
        leaf('Branch Grace Days (Locking)', 'Tools', 'VOUCHERLOCK'),
        leaf('Branch Logged History', 'Tools', 'BranchLoggedHistory'),
        leaf('Branch Location Logged History', 'Tools', 'BranchLocationLoggedHistory'),
        leaf('Import Masters', 'Tools', 'ImportMasterData'),
        leaf('Import Branches', 'Tools', 'ImportBranches'),
        leaf('Country', 'Data', 'CountryView'),
        leaf('State', 'Data', 'StateView'),
    ]),
    group('Ticketing', 'TroubleTicket', 'TicketView', [
        leaf('Post Ticket', 'TroubleTicket', 'TrackTicket'),
    ]),
    group('Help', 'Help', 'Help', [
        leaf('User Manual', 'Help', 'UserManual'),
    ]),
];

// Branch Office login: Home / Finance / Utilities (same web layout as Head Office).
// Top items open a left sidebar; each section lists the screens of the legacy desktop app.
// `icon` / `tone` style the sidebar section header.
function branchSection(module, title, key, icon, tone, items) {
    return {
        id: `branch.${module}.${key}`,
        title,
        icon,
        tone,
        children: items.map(([itemTitle, itemKey]) => ({ id: `branch.${module}.${key}.${itemKey}`, title: itemTitle })),
    };
}

export const branchOfficeMenu = [
    home,
    { id: 'branch.finance', title: 'Finance', children: [
        branchSection('finance', 'Masters', 'masters', 'masters', 'amber', [['Legal Entities', 'legalentities'], ['Project Category', 'projectcategory'], ['Project', 'project'], ['Ledger Group', 'ledgergroup'], ['Ledger', 'ledger'], ['Cash Ledger', 'cashledger'], ['Bank and Branch', 'bankandbranch'], ['Bank Account', 'bankaccount'], ['Sundry Creditors & Debtors', 'sundrycreditorsdebtors'], ['Cost Centre Category', 'costcentrecategory'], ['Cost Centre', 'costcentre'], ['Country', 'country'], ['State', 'state'], ['Lock Type', 'locktype'], ['Donor', 'donor'], ['Purpose', 'purpose'], ['Statistics Type', 'statisticstype'], ['Vendors', 'vendors']]),
        branchSection('finance', 'Accounting', 'accounting', 'accounting', 'orange', [['Receipt', 'receipt'], ['Payment', 'payment'], ['Contra', 'contra'], ['Journal', 'journal'], ['Bank Reconciliation', 'bankreconciliation'], ['Budget - Annual', 'budgetannual']]),
        branchSection('finance', 'Investment', 'investment', 'investment', 'red', [['Investment Ledger', 'investmentledger'], ['Opening Balance', 'openingbalance'], ['Investment', 'investment'], ['Renewal', 'renewal'], ['Post Interest', 'postinterest'], ['Withdrawal', 'withdrawal'], ['Closed Investment', 'closedinvestment'], ['Register', 'register']]),
        branchSection('finance', 'Options', 'options', 'options', 'slate', [['Refresh Ledger Balance', 'refreshledgerbalance'], ['Define Voucher Number', 'definevouchernumber'], ['Regenerate Voucher Number', 'regeneratevouchernumber'], ['Map Accounts', 'mapaccounts'], ['Ledger Options', 'ledgeroptions'], ['Lock Voucher', 'lockvoucher'], ['Authorize Voucher', 'authorizevoucher'], ['Bulk Update Narration', 'bulkupdatenarration'], ['Settings', 'settings']]),
        branchSection('finance', 'Views', 'views', 'views', 'teal', [['Vouchers', 'vouchers'], ['Journal', 'journal'], ['Audit Log', 'auditlog']]),
    ] },
    { id: 'branch.utilities', title: 'Utilities', children: [
        branchSection('utilities', 'Configuration', 'configuration', 'configuration', 'slate', [['Institute Info', 'instituteinfo'], ['Settings', 'settings'], ['Transaction Period', 'transactionperiod']]),
        branchSection('utilities', 'User Management', 'usermanagement', 'usermanagement', 'blue', [['User', 'user'], ['User Role', 'userrole'], ['User Rights', 'userrights'], ['Manage User Security', 'manageusersecurity']]),
        branchSection('utilities', 'Data Management', 'datamanagement', 'datamanagement', 'dark', [['Manage Multi Branch', 'managemultibranch'], ['Backup Database', 'backupdatabase'], ['Restore Database', 'restoredatabase']]),
        branchSection('utilities', 'Head Office Interface', 'headofficeinterface', 'headofficeinterface', 'green', [['License Key', 'licensekey'], ['Import Masters', 'importmasters'], ['Export Vouchers', 'exportvouchers'], ['Map Headoffice Ledger', 'mapheadofficeledger'], ['Upload Database', 'uploaddatabase']]),
    ] },
];

// Find an item and its path (top-level first) by id.
export function findMenuPath(menu, id, path = []) {
    for (const item of menu) {
        const next = [...path, item];
        if (item.id === id) {
            return next;
        }
        if (item.children) {
            const found = findMenuPath(item.children, id, next);
            if (found) {
                return found;
            }
        }
    }
    return null;
}
