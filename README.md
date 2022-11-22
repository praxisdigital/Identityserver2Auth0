# Identityserver2Auth0

Credit: [blogpost](https://community.auth0.com/t/migrate-users-from-identity-server-v3-asp-net-core-to-auth0/90958)

## Migration tool
This little tool will export the users in the IdentityServer database with their hashed password, then you can import the users into Auth0 by using their import tool. The user can afterwords login with Auth0 with their old password.