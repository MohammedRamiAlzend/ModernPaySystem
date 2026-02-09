export interface DecodedToken {
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": string;
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": string;
    permission: string[];
    role: string | string[];
    exp: number;
    iss?: string;
    aud?: string;
}

export interface LoginCredentials {
    username: string;
    password: string;
}

// الـ API يرجع التوكن مباشرة كسلسلة نصية
export type LoginResponse = string;
