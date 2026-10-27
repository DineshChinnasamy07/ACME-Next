# Bosco Auth endpoints

Base URL: `http://localhost:5118`

Swagger: `http://localhost:5118/swagger`

Gateway: `http://localhost:5080/auth` followed by the same path. Example: `http://localhost:5080/auth/api/auth/login`

These three actions live on `AuthController`. Login does not need a token. Logout does.

The signed-in time comes from `Jwt:ExpiryMinutes` in `appsettings.json`. It is **60** minutes. That time is stored in the token at login. Changing the setting later does not change a token that was already issued. Log in again after a change.

## POST /api/auth/login

Purpose: sign in from Swagger or any JSON client.

Auth: none.

Content type: `application/json`

### Request

| Field | Required | Meaning |
|---|---|---|
| `headOfficeCode` | Yes | `portal` for a portal user, or a real head office code |
| `username` | Yes | `user_info.USER_NAME` |
| `password` | Yes | The password the user typed. The API encrypts it. |

Portal admin already inserted in `acperp`:

```json
{
  "headOfficeCode": "portal",
  "username": "admin",
  "password": "admin123"
}
```

`portal` is the login context. The portal user row has an empty head office code and an empty branch code. A blank `headOfficeCode` is rejected. An unknown office code is not treated as portal.

### Success: 200

```json
{
  "accessToken": "<jwt>",
  "tokenType": "Bearer",
  "expiresInMinutes": 60,
  "passwordChangeRequired": false,
  "user": {
    "userId": 1,
    "userName": "admin",
    "roleId": 1,
    "userRole": "Admin",
    "userContext": "Portal",
    "headOfficeCode": "",
    "branchCode": "",
    "isPortalUser": true,
    "isHeadOfficeUser": false,
    "isBranchOfficeUser": false,
    "isHeadOfficeAdminUser": false,
    "isBranchOfficeAdminUser": false,
    "passwordChangeRequired": false
  }
}
```

The token carries user id, username, role, head office code, branch code, user context, and password status. It does not carry the password.

Send it on later calls as:

```http
Authorization: Bearer <accessToken>
```

### Who the user is

| Context | Head office code on the user | Branch code on the user |
|---|---|---|
| Portal | empty | empty |
| Head office | set | empty |
| Branch | set | set |

Role is separate. `roleId` 1 is Admin. `roleId` 2 is Branch Admin. A head-office admin is a head-office user with role 1. A branch admin is a branch user with role 2. The account must be active (`STATUS = 1`). Exactly one password match is accepted.

`passwordChangeRequired` is true when `PASSWORD_STATUS` is 0. The token is still returned. Protected Auth controllers require `password_change_required` to be `false`.

### Failures

| Status | When | Body |
|---|---|---|
| 400 | Head office code, username, or password is missing | `{ "message": "..." }` |
| 400 | The head office code is not `portal` and is not one head office row | `{ "message": "Invalid Head Office Code." }` |
| 401 | Unknown user, wrong password, inactive user, wrong office, or more than one match | `{ "message": "Invalid Username or Password!" }` |

## POST /api/auth/login/public

Purpose: the same login for the old public website. The old site posted form fields `hocode`, `username`, and `password`. This action uses the same checks as `/api/auth/login`. It does not redirect to the old website.

Auth: none.

### Form request

Content type: `application/x-www-form-urlencoded` or `multipart/form-data`

| Field | Required | Meaning |
|---|---|---|
| `hocode` | Yes | Same value as `headOfficeCode` |
| `username` | Yes | Username |
| `password` | Yes | Typed password |

### JSON request

Content type: `application/json`

Use the same body as `/api/auth/login` (`headOfficeCode`, `username`, `password`).

In Swagger, choose the content type before Execute. A call with no content type and no body returns 400, not the old `Incorrect Content-Type` exception.

Success and failure bodies match `/api/auth/login`.

## POST /api/auth/logout

Purpose: tell the caller to drop the bearer token.

Auth: bearer token required. A token that still needs a password change can call this.

### Request

No body.

```http
Authorization: Bearer <accessToken>
```

### Success: 200

```json
{
  "success": true,
  "tokenRevoked": false,
  "message": "Signed out. Discard the bearer token. It remains valid until it expires."
}
```

`tokenRevoked` is false because Auth has no revocation table. The server does not cancel the token when the minute or the 60 minutes elapse by itself. The next protected call after `exp` returns 401. Swagger keeps the token in Authorize until you clear it.

### Failure

| Status | When |
|---|---|
| 401 | Missing token, bad token, or expired token |
