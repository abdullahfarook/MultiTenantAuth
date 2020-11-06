import { OAuthModuleConfig } from 'angular-oauth2-oidc';
import { environment } from '@env/environment';
export function setEnv() {
  var env = window["__env"];
  if (env) {
    if (window["__env"]["apiUrl"]) {
      environment.ResourceServer = window["__env"]["apiUrl"] + "/";
      environment.IssuerUri = window["__env"]["apiUrl"];
      environment.Uri = window["__env"]["apiUrl"];
    }
  }
}
export const authModuleConfig: OAuthModuleConfig = {
    resourceServer: {
        allowedUrls: [environment.ResourceServer],
        sendAccessToken: true
    }
};
