import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  AccountService
} from '../../../../core/services/account.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthenticationRequest, AuthenticationResponse } from '../../../../core/models/authentication.model';
import { BaseResponse } from '../../../../core/models/update-device.model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: false,
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.loginForm = this.fb.group({
      userName: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    const request: AuthenticationRequest = this.loginForm.value;

    this.accountService.authenticate(request).subscribe({
      next: (response: BaseResponse<AuthenticationResponse>) => {
        this.loading = false;
        if (response.success) {
          const base64Image = response.data.picture.length > 1
            ? `data:image/png;base64,${response.data.picture}`
            : '';

          this.accountService.saveUserInfo(
            response.data.token,
            response.data.userName,
            base64Image,
            response.data.role,
          );


          this.snackBar.open('Login successful!', 'Close', { duration: 3000 });
          this.router.navigate(['/devices']);
        } else {
          this.snackBar.open(response.message, 'Close', { duration: 3000 });
        }
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('An error occurred. Please try again.', 'Close', {
          duration: 3000,
        });
      },
    });
  }
}
