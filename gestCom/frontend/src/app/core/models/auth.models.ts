export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  nom: string;
  prenom: string;
  codeEntreprise?: string;
}

export interface User {
  id: string;
  email: string;
  nom: string;
  prenom: string;
  role: string;
  codeEntreprise?: string;
  entrepriseId?: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
  expiresAt: Date;
}

export interface DecodedToken {
  sub: string;
  email: string;
  role: string;
  nom: string;
  prenom: string;
  codeEntreprise?: string;
  exp: number;
  iat: number;
}
