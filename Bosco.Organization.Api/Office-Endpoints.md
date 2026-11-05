# Organization Office — Method Purpose Document

Service: **Bosco.Organization** (EF Core only). Portal DB: `admin_Portal` via `ConnectionStrings:AcpErp`.

Auth: every Head Office, Country, and Branch Office route requires JWT (`Authorization: Bearer`). Get the token from Auth `POST /api/auth/login`. Swagger UI (`/swagger` and `/openapi`) requires Basic Auth from `Swagger:Username` / `Swagger:Password` in this API `appsettings.json`. Then use Swagger Authorize with the JWT to call APIs.

Follow this document for method names and purpose. Do not invent alternate names when a method is listed here.

Status values: `1` = Created, `2` = Activated, `3` = DeActivated.

---

## A. Head Office (`HeadOfficeSystem`)

| Method | Purpose | Status |
|--------|---------|--------|
| `FetchHeadOfficeDetails` | List all portal head offices for the grid | Done |
| `HeadOfficeDetailById` | Load one head office by `HEAD_OFFICE_ID` for edit/view | Done |
| `SaveHeadOfficeDetails` | Add (`HeadOfficeId == 0`) or update existing portal `head_office` row | Done |
| `UpdateOfficeStatus` | Set status Created / Activated / DeActivated by head office code | Done |
| `DeleteHeadOfficeDetails` | Delete portal `head_office` by head office code | Done |
| `FetchActiveHeadOfficeDetails` | List only Activated (`STATUS = 2`) HOs (Branch Office dropdown) | Done |
| `FetchHeadOfficeType` | Type dropdown from `head_office_type` (`TYPE_ID`, `TYPE`) | Done |
| `FetchCountry` | Country dropdown from `country` (`COUNTRY_ID`, `COUNTRY_CODE`, `COUNTRY`) — `CountryController` | Done |
| `FetchStateByCountry` | State dropdown by country — `CountryController` | Done |
| `FetchBranchByHeadOffice` | Active main branches under HO (`STATUS = 2`, `IS_SUBBRANCH = 0`); also blocks HO deactivate | Done |
| `UpdateDatabase` | Update HOST_NAME / DB_NAME / USERNAME / PASSWORD by HO code | Done |
| `UpdateUserCreatedStatus` | Mark HO admin user created / communicated (`USER_CREATED_STATUS` 0–3) on portal | Done |
| Dual-write HO row (Phase A) | After portal `SaveHeadOfficeDetails`, sync `head_office` into HO tenant DB (no HOST/DB/USER/PWD) | Done |
| Activate + HO admin (Phase B) | On activate (`STATUS` 1→2): dual-write status, create HO admin user, set `USER_CREATED_STATUS=2` (no mail) | Done |
| DB restore (Phase C) | Create HO DB if missing + run `HeadOfficeTenant:TemplateSqlPath` PG script on new HO save | Done (code); needs `ACPERP_HO_PG.sql` file |

### Head Office HTTP API

| HTTP | Route | System method | Purpose |
|------|-------|---------------|---------|
| GET | `/api/headoffices` | `FetchHeadOfficeDetails` | List all portal head offices for the grid |
| GET | `/api/headoffices/active` | `FetchActiveHeadOfficeDetails` | List Activated HOs only (`STATUS = 2`) for Branch dropdown |
| GET | `/api/headoffices/types` | `FetchHeadOfficeType` | List head office types for the Type dropdown |
| GET | `/api/headoffices/{headOfficeId}` | `HeadOfficeDetailById` | Load one head office by id for edit/view |
| GET | `/api/headoffices/{headOfficeCode}/branches` | `FetchBranchByHeadOffice` | List active main branches under HO |
| POST | `/api/headoffices` | `SaveHeadOfficeDetails` | Add or update a portal head office |
| PUT | `/api/headoffices/{headOfficeCode}/status` | `UpdateOfficeStatus` | Set status; reject DeActivated (3) if active branches; on first activate create HO admin (returns `generatedAdminPassword`) |
| PUT | `/api/headoffices/{headOfficeCode}/database` | `UpdateDatabase` | Update HO DB connection fields on portal |
| PUT | `/api/headoffices/{headOfficeCode}/user-created-status` | `UpdateUserCreatedStatus` | Set USER_CREATED_STATUS (0–3) on portal HO |
| DELETE | `/api/headoffices/{headOfficeCode}` | `DeleteHeadOfficeDetails` | Delete portal head office by code |

### Country HTTP API (`CountryController`)

| HTTP | Route | System method | Purpose |
|------|-------|---------------|---------|
| GET | `/api/countries` | `FetchCountry` | List countries for the Country dropdown |
| GET | `/api/countries/{countryId}/states` | `FetchStateByCountry` | List states by country for the State dropdown |

### Save rules (portal)

- Code required, max 6 characters, stored lowercase.
- Name required.
- New row: `STATUS = 1` (Created), `CREATED_DATE = UtcNow`, `USER_CREATED_STATUS = 0` if not set.
- Update: does not wipe password/host unless sent; sets `MODIFIED_DATE`.
- Duplicate `HEAD_OFFICE_CODE` rejected.

---

## B. Branch Office (`BranchOfficeSystem`)

| Method | Purpose | Status |
|--------|---------|--------|
| `FetchBranchOfficeDetails` | List all portal branch offices for the grid | Done |
| `FillBranchOfficeDetails` | Load one branch office by `BRANCH_OFFICE_ID` for edit/view | Done |
| `SaveBranchOfficeDetails` | Add (`BranchOfficeId == 0`) or update existing portal `branch_office` row | Done |
| `UpdateOfficeStatus` | Set status Created / Activated / DeActivated by full branch office code | Done |
| `DeleteBranchOfficeDetails` | Delete portal `branch_office` by full branch office code (license delete later) | Done |
| HO dropdown | Only Activated head offices | Done (`GET /api/headoffices/active`) |

### Branch Office HTTP API

| HTTP | Route | System method | Purpose |
|------|-------|---------------|---------|
| GET | `/api/branchoffices` | `FetchBranchOfficeDetails` | List all portal branch offices for the grid |
| GET | `/api/branchoffices/{branchOfficeId}` | `FillBranchOfficeDetails` | Load one branch office by id for edit/view |
| POST | `/api/branchoffices` | `SaveBranchOfficeDetails` | Add or update a portal branch office |
| PUT | `/api/branchoffices/{branchOfficeCode}/status` | `UpdateOfficeStatus` | Set status Created / Activated / DeActivated by full branch office code |
| DELETE | `/api/branchoffices/{branchOfficeCode}` | `DeleteBranchOfficeDetails` | Delete portal branch office by full code (no license delete this pass) |

List row: `branchOfficeId`, `headOfficeCode`, `code` (`BRANCH_PART_CODE`), `name`, `deploymentType`, `systemType` (Standalone / Client-Server), `status`, `officeStatus` (Created / Activated / DeActivated), `isSubBranch`, `subBranch` (Yes / No). Ordered by status, then branch office code.

Get-by-id row: `branchOfficeId`, `branchOfficeCode`, `branchOfficeName`, `headOfficeCode`, `branchPartCode`, `deploymentType`, `status`, `isSubBranch`, `address`, `countryId`, `stateId`, `pincode`, `phoneNo`, `mobileNo`, `countryCode`, `branchEmail`, `city`, `userCreatedStatus`, `branchKeyCode`, `personIncharge`, `thirdPartyCode`. Missing id returns 404.

Save rules (portal only, no HO tenant dual-write):
- Head Office required; must exist and be Activated (`STATUS = 2`).
- Code (`BRANCH_PART_CODE`) 6–10 characters, stored lowercase, not equal to HO code.
- Name required, max 50.
- Full `BRANCH_OFFICE_CODE` = HO code + part code, lowercase.
- New row: `STATUS = 1` (Created), `USER_CREATED_STATUS = 0`, `CREATED_DATE = UtcNow`.
- Update: does not change status; sets `MODIFIED_DATE`.
- Duplicate full code rejected.

Status / delete rules (portal only, no HO tenant dual-write):
- Status required; must be 1, 2, or 3.
- Status lookup is by full `BRANCH_OFFICE_CODE`.
- Delete is portal row only this pass. License delete is later.
- Missing code or missing row returns the catalog message.

User-facing messages: `MessageCatalog.Office` keys in `Bosco.Common/Resources/OfficeMessages.resx` (+ `OfficeMessages.fr-FR.resx`). Controllers and systems use `MessageRender.GetMessage(key)`.

Swagger group: `BranchOffice` on `BranchOfficeController`. XML comments are on list, get-by-id, save, status, and delete.

---

## C. Later (not this Branch pass)

- Dual-write branch row into HO tenant database
- First Activate creates Branch Admin user in HO DB
- Delete license before delete branch
- Email / license key screens
