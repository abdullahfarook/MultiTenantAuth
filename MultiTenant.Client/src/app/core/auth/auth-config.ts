import { environment } from '@env/environment';
import { AuthConfig } from 'angular-oauth2-oidc';
import { setEnv } from './auth-module-config';

export const authConfig: AuthConfig = getObj();
function getObj(): AuthConfig {
  // setEnv();
  return {
    issuer: environment.IssuerUri,
    clientId: 'IS4-Admin',
    requireHttps: environment.RequireHttps,
    redirectUri: environment.Uri + '/login-callback',
    scope: 'openid profile email jp_api',
    responseType: 'code',
    silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
    useSilentRefresh: true, // Needed for Code Flow to suggest using iframe-based refreshes
    silentRefreshTimeout: 5000, // For faster testing
    timeoutFactor: 0.25, // For faster testing
    sessionChecksEnabled: true,
    showDebugInformation: true, // Also requires enabling "Verbose" level in devtools
    clearHashAfterLogin: false, // https://github.com/manfredsteyer/angular-oauth2-oidc/issues/457#issuecomment-431807040,
    nonceStateSeparator: 'semicolon', // Real semicolon gets mangled by IdentityServer's URI encoding
    customQueryParams: {
      acr_values: 'tenant:tenant2',
    },
  };
}
