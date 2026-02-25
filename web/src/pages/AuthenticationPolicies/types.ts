import type { AuthenticationSchemeType } from '../../types';

export interface AuthenticationPolicyFormData {
  name: string;
  description: string;
  type: AuthenticationSchemeType | '';
  enabled: boolean;
  // JWT Bearer fields
  jwtAuthority: string;
  jwtAudience: string;
  jwtRequireHttpsMetadata: boolean;
  jwtSaveToken: boolean;
  jwtValidIssuers: string[];
  jwtValidAudiences: string[];
  jwtValidateIssuer: boolean;
  jwtValidateAudience: boolean;
  jwtValidateLifetime: boolean;
  jwtValidateIssuerSigningKey: boolean;
  jwtClockSkew: string;
  // OpenID Connect fields
  oidcAuthority: string;
  oidcClientId: string;
  oidcClientSecret: string;
  oidcResponseType: string;
  oidcRequireHttpsMetadata: boolean;
  oidcSaveTokens: boolean;
  oidcGetClaimsFromUserInfoEndpoint: boolean;
  oidcScope: string[];
  oidcClockSkew: string;
}
