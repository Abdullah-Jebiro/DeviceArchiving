export interface AuthenticationResponse {
    token: string;
    userName: string;
    picture: string;
    role: string;
}

export interface AuthenticationRequest {
  email: string;
  password: string;
}


export const UserRole = {
  Admin: 'Admin',
  User: 'User',
  Manager: 'Manager'
} as const;
