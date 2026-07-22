import { Component, computed, inject } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile',
  standalone: true,
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
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
