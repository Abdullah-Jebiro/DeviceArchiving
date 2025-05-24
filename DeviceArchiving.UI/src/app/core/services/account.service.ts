import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthenticationResponse } from '../models/authentication.model';
import { Router } from '@angular/router';
import { BaseResponse } from '../models/update-device.model';

export interface AuthenticationRequest {
  email: string;
  password: string;
}



@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private readonly apiUrl: string = `${environment.apiBaseUrl}/api/Account`;

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) { }

  authenticate(request: AuthenticationRequest): Observable<BaseResponse<AuthenticationResponse>> {
    return this.http.post<BaseResponse<AuthenticationResponse>>(
      `${this.apiUrl}/authenticate`,
      request
    ).pipe(
      catchError(this.handleError)
    );
  }

  addUser(request: AuthenticationRequest): Observable<BaseResponse<string>> {
    return this.http.post<BaseResponse<string>>(
      `${this.apiUrl}/register`,
      request
    ).pipe(
      catchError(this.handleError)
    );
  }



  saveUserInfo(token: string, userName: string, base64Image: string): void {
    sessionStorage.setItem('authToken', token);
    sessionStorage.setItem('userName', userName);
    sessionStorage.setItem('userPicture', base64Image);
  }

  getUserInfo(): { token: string | null; userName: string | null; picture: string | null } {
    return {
      token: sessionStorage.getItem('authToken'),
      userName: sessionStorage.getItem('userName'),
      picture: sessionStorage.getItem('userPicture')
    };
  }

  getToken(): string | null {
    return sessionStorage.getItem('authToken');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  clearSession(): void {
    sessionStorage.clear();
  }

  logout(): void {
    this.clearSession();
    this.router.navigate(['/account/login']);
  }

  private handleError(error: any): Observable<never> {
    console.error('An error occurred:', error);
    const errorMessage = error.error?.message || 'An unexpected error occurred';
    alert(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}