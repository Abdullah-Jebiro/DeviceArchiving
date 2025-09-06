import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BaseResponse } from '../../../../core/models/update-device.model';
import { AccountService } from '../../../../core/services/account.service';
import { AuthenticationRequest } from '../../../../core/models/authentication.model';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css'],
  standalone: false,
})
export class SignupComponent {
  signupForm: FormGroup;
  loading = false;
  hidePassword = true;
  
  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.signupForm = this.fb.group({
      userName: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  togglePasswordVisibility(): void {
    this.hidePassword = !this.hidePassword;
  }

  onSubmit(): void {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    const request: AuthenticationRequest = this.signupForm.value;

    this.accountService.addUser(request).subscribe({
      next: (response: BaseResponse<string>) => {
        this.loading = false;
        if (response.success) {
          this.snackBar.open('Signup successful! Please log in.', 'Close', { duration: 3000 });
    
          this.router.navigate(['/account/login']);
    
        } else {
          this.snackBar.open(response.message, 'Close', { duration: 3000 });
        }
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('An error occurred. Please try again.', 'Close', { duration: 3000 });
      }
    });
  }
}