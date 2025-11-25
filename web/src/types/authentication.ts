export type AuthenticationSchemeType = 'JwtBearer' | 'OpenIdConnect';

export interface AuthenticationPolicy {
  id: string;
  name: string;
  type: AuthenticationSchemeType;
  enabled: boolean;
  description?: string;
  jwtBearer?: JwtBearerConfig;
  openIdConnect?: OpenIdConnectConfig;
  createdAt: string;
  updatedAt: string;
}

export interface JwtBearerConfig {
  authority: string;
  audience: string;
  requireHttpsMetadata: boolean;
  saveToken: boolean;
  validIssuers?: string[];
  validAudiences?: string[];
  validateIssuer: boolean;
  validateAudience: boolean;
  validateLifetime: boolean;
  validateIssuerSigningKey: boolean;
  clockSkew?: number;
}

export interface OpenIdConnectConfig {
  authority: string;
  clientId: string;
  clientSecret: string;
  responseType?: string;
  requireHttpsMetadata: boolean;
  saveTokens: boolean;
  getClaimsFromUserInfoEndpoint: boolean;
  scope?: string[];
  clockSkew?: number;
}

