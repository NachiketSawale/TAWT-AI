export interface PatLoginRequest {
  patToken: string;
}

export interface PatLoginResponse {
  success: boolean;
  accessToken?: string;
  expiresAtUtc?: string;
  displayName?: string;
  emailAddress?: string;
  organization?: string;
  message: string;
}

export interface AuthSession {
  accessToken: string;
  expiresAtUtc: string;
  displayName: string;
  emailAddress: string;
  organization: string;
}
