import { Component, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  readonly hideToken = signal(true);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly form;
  constructor(private readonly fb: FormBuilder, private readonly router: Router, private readonly authService: AuthService) {
    this.form = this.fb.group({
      patToken: ['', [Validators.required]]
    });
  }

  toggleTokenVisibility(event: MouseEvent): void {
    event.preventDefault();
    this.hideToken.update((value) => !value);
  }

  signInWithMicrosoft(): void {
    this.errorMessage.set('Microsoft SSO is not configured yet. Please sign in with your Personal Access Token.');
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.isSubmitting.set(true);

    const { patToken } = this.form.getRawValue();

    this.authService.login({ patToken: patToken ?? '' }).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);
        if (response.success) {
          this.successMessage.set('Token validated successfully.');
          this.router.navigate(['profile']);
        } else {
          this.errorMessage.set(response.message ?? 'Invalid Personal Access Token. Please check and try again.');
        }
      },
      error: (err) => {
        this.isSubmitting.set(false);
        if (err?.status === 0) {
          this.errorMessage.set('Unable to reach the authentication service. Please check your connection and try again.');
        } else if (err?.status === 401 || err?.status === 403) {
          this.errorMessage.set('Invalid or expired Personal Access Token. Please check and try again.');
        } else {
          this.errorMessage.set(err?.error?.message ?? 'Something went wrong while validating the token. Please try again.');
        }
      }
    });
  }
}
