import { Component, computed, inject } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile',
  standalone: true,
  template: `
    <div class="profile-page">
      <header class="top-bar">
        <span class="top-bar-title">Welcome</span>
      </header>

      <div class="profile-backdrop">
        <div class="profile-card">
          <div class="card-content">
            <h2>Hello, {{ displayName() }}</h2>
            <p class="subtext">You are now logged in.</p>

            <button class="logout-button" (click)="logout()">Log out</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
    $bg-dark: #0f172a;
    $card-bg: #ffffff;
    $card-border: #e6eef8;
    $primary: #1f6feb;

    .profile-page {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
    }

    .top-bar {
      padding: 16px 24px;
      background: $bg-dark;
      color: #f8fafc;
      font-size: 1.1rem;
      font-weight: 600;
    }

    .profile-backdrop {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 24px;
    }

    .profile-card {
      width: min(560px, 100%);
      background: $card-bg;
      border-radius: 20px;
      border: 1px solid $card-border;
      box-shadow: 0 24px 70px rgba(15, 23, 42, 0.25);
      overflow: hidden;
    }

    .card-content {
      padding: 32px;
      text-align: center;
    }

    h2 {
      margin: 0 0 8px 0;
      font-size: 1.6rem;
      color: #0f172a;
    }

    .subtext {
      color: #64748b;
      margin-bottom: 20px;
    }

    .logout-button {
      padding: 12px 20px;
      border-radius: 10px;
      border: none;
      background: $primary;
      color: white;
      cursor: pointer;
      font-weight: 600;
    }
    `
  ]
})
export class ProfileComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly session = this.authService.session;

  readonly displayName = computed(() =>
    this.session() ? (this.session()!.displayName || this.session()!.emailAddress || 'User') : 'User'
  );

  logout(): void {
    this.authService.logout();
    this.router.navigate(['login']);
  }
}
