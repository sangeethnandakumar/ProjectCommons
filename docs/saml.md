# SAML Workflow

> SAML works only with Browser. So we can't make an API that uses OAuth 2.0.

> A small frontend is needed to complete the assertion

# SAML + JWT Authentication Flow (Swimlane)

| Step | User          | Frontend (React)     | Backend (.NET)         | Identity Provider (SAML - e.g. Entra AD) | API (.NET)                |
|------|---------------|----------------------|-------------------------|-------------------------------------------|---------------------------|
| 1    | Opens the app |                      |                         |                                           |                           |
| 2    |               | Checks token → None  |                         |                                           |                           |
| 3    |               | Redirects to `/saml/login` |                   |                                           |                           |
| 4    |               |                      | Builds SAMLRequest & redirects to IdP SSO URL |                             |                           |
| 5    |               |                      |                         | Shows login page (if not logged in)       |                           |
| 6    | Logs in       |                      |                         | Authenticates user (password/MFA)         |                           |
| 7    |               |                      |                         | Sends SAMLResponse (via HTTP POST) to ACS |                           |
| 8    |               |                      | Receives `/saml/acs` with SAMLResponse     |                                           |                           |
| 9    |               |                      | Validates SAML XML signature               |                                           |                           |
| 10   |               |                      | Extracts user attributes (email, name, etc.) |                                         |                           |
| 11   |               |                      | Issues JWT (with claims)                   |                                           |                           |
| 12   |               | Receives redirect with `?token=...`            |                                             |                             |                           |
| 13   |               | Stores JWT (localStorage/cookie)              |                                             |                             |                           |
| 14   |               | Sends API request with `Authorization: Bearer <JWT>` |         |                                           |                           |
| 15   |               |                      |                         |                                           | Validates JWT, extracts claims |
| 16   |               |                      |                         |                                           | Responds with secure data |



![image](https://github.com/user-attachments/assets/14706a3b-739f-429d-8817-f28aefdaab90)
