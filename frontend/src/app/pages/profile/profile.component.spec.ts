import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { signal } from '@angular/core';
import { ProfileComponent } from './profile.component';
import { AuthService } from '../../services/auth.service';
import { AuthSession } from '../../models/auth.model';

describe('ProfileComponent', () => {
  let logoutMock: ReturnType<typeof vi.fn>;
  let navigateMock: ReturnType<typeof vi.fn>;
  let sessionSignal: ReturnType<typeof signal<AuthSession | null>>;

  const createComponent = () => {
    const fixture = TestBed.createComponent(ProfileComponent);
    fixture.detectChanges();
    return fixture.componentInstance;
  };

  beforeEach(() => {
    logoutMock = vi.fn();
    navigateMock = vi.fn();
    sessionSignal = signal<AuthSession | null>(null);

    TestBed.configureTestingModule({
      imports: [ProfileComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { logout: logoutMock, session: sessionSignal } }
      ]
    });

    TestBed.overrideProvider(Router, { useValue: { navigate: navigateMock } });
  });

  it('falls back to "User" when there is no session', () => {
    const component = createComponent();
    expect(component.displayName()).toBe('User');
  });

  it('prefers displayName when present', () => {
    sessionSignal.set({
      accessToken: 't',
      expiresAtUtc: new Date().toISOString(),
      displayName: 'Jane Doe',
      emailAddress: 'jane@example.com',
      organization: 'ribdev'
    });
    const component = createComponent();
    expect(component.displayName()).toBe('Jane Doe');
  });

  it('falls back to emailAddress when displayName is empty', () => {
    sessionSignal.set({
      accessToken: 't',
      expiresAtUtc: new Date().toISOString(),
      displayName: '',
      emailAddress: 'jane@example.com',
      organization: 'ribdev'
    });
    const component = createComponent();
    expect(component.displayName()).toBe('jane@example.com');
  });

  it('logs out and navigates to /login', () => {
    const component = createComponent();

    component.logout();

    expect(logoutMock).toHaveBeenCalled();
    expect(navigateMock).toHaveBeenCalledWith(['login']);
  });
});
