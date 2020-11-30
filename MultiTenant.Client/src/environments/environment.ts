// The file contents for the current environment will overwrite these during build.
// The build system defaults to the dev environment which uses `environment.ts`, but if you do
// `ng build --env=prod` then `environment.prod.ts` will be used instead.
// The list of which env maps to which file can be found in `.angular-cli.json`.

export const environment = {
  production: false,

  // ResourceServer: 'https://tenant1.tenants.local:5000/',

  // ResourceServer: 'https://tenants.local:5000/tenant2',
  IssuerUri: 'https://localhost:5000',
  // IssuerUri: 'https://tenant1.tenants.localhost:5000',

  // ResourceServer: 'https://localhost:5000/',

  //IssuerUri: "https://localhost:5000",
  RequireHttps: false,
  Uri: 'http://localhost:4200',
  defaultTheme: 'E',
  version: '3.0.3',
};
