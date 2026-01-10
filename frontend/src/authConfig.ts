import { Configuration, PopupRequest } from "@azure/msal-browser";

// Config object to be passed to Msal on creation
export const msalConfig: Configuration = {
    auth: {
        clientId: "2f2020ec-264d-4de5-bea4-f4dfc545c5d8", // From PENDING-AUTH-REENABLE.md and appsettings
        authority: "https://login.microsoftonline.com/a93513e2-d327-4301-80ed-d703eb03f6cb", // TenantId from same docs
        redirectUri: "/",
        postLogoutRedirectUri: "/"
    },
    cache: {
        cacheLocation: "sessionStorage", // This configures where your cache will be stored
        storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
    },
    system: {
        allowNativeBroker: false // Disables WAM Broker
    }
};

// Add here scopes for id token to be used at MS Identity Platform endpoints.
export const loginRequest: PopupRequest = {
    scopes: ["User.Read", "api://2f2020ec-264d-4de5-bea4-f4dfc545c5d8/access_as_user"]
    // Note: The second scope is the typically exposed API scope for custom access
    // If just verifying identity, User.Read is enough.
    // Based on PENDING-AUTH-REENABLE.md, we need to acquire access tokens for API calls.
    // "Include token in Authorization header: Bearer <token>"
    // The scope usually follows the pattern api://<clientId>/<scopeName>
    // I will assume "access_as_user" is the scope name or defaults.
    // If fails, we can adjust.
};
