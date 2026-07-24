import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../../services/auth.service';
import { PatLoginResponse } from '../../models/auth.model';

describe('LoginComponent', () => {
  let loginMock: ReturnType<typeof vi.fn>;
  let navigateMock: ReturnType<typeof vi.fn>;

  const successResponse: PatLoginResponse = {
    success: true,
    accessToken: 'token-123',
    expiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
    displayName: 'Jane Doe',
    emailAddress: 'jane.doe@example.com',
    organization: 'ribdev',
    message: ''
  };

  const createComponent = () => {
    const fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
    return fixture.componentInstance;
  };

  beforeEach(() => {
    loginMock = vi.fn();
    navigateMock = vi.fn();

    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { login: loginMock } }
      ]
    });

    TestBed.overrideProvider(Router, { useValue: { navigate: navigateMock } });
  });

  it('marks the form as touched and does not call AuthService when the token is empty', () => {
    const component = createComponent();

    component.submit();

    expect(loginMock).not.toHaveBeenCalled();
    expect(component.form.controls.patToken.touched).toBe(true);
  });

  it('navigates to /profile and shows a success message on a successful login', () => {
    loginMock.mockReturnValue(of(successResponse));
    const component = createComponent();
    component.form.controls.patToken.setValue('pat-abc');

    component.submit();

    expect(loginMock).toHaveBeenCalledWith({ patToken: 'pat-abc' });
    expect(component.isSubmitting()).toBe(false);
    expect(component.successMessage()).toContain('validated successfully');
    expect(navigateMock).toHaveBeenCalledWith(['profile']);
  });

  it('shows the backend message when the response reports failure', () => {
    loginMock.mockReturnValue(of({ success: false, message: 'Invalid token' } satisfies PatLoginResponse));
    const component = createComponent();
    component.form.controls.patToken.setValue('bad-pat');

    component.submit();

    expect(component.errorMessage()).toBe('Invalid token');
    expect(navigateMock).not.toHaveBeenCalled();
  });

  it('shows an "invalid or expired" message on a 401 response', () => {
    loginMock.mockReturnValue(throwError(() => ({ status: 401 })));
    const component = createComponent();
    component.form.controls.patToken.setValue('pat-abc');

    component.submit();

    expect(component.errorMessage()).toContain('Invalid or expired');
  });

  it('shows a connectivity message when the request cannot reach the server', () => {
    loginMock.mockReturnValue(throwError(() => ({ status: 0 })));
    const component = createComponent();
    component.form.controls.patToken.setValue('pat-abc');

    component.submit();

    expect(component.errorMessage()).toContain('Unable to reach the authentication service');
  });

  it('toggles token visibility', () => {
    const component = createComponent();
    const event = new MouseEvent('click');
    const preventDefaultSpy = vi.spyOn(event, 'preventDefault');

    expect(component.hideToken()).toBe(true);

    component.toggleTokenVisibility(event);

    expect(preventDefaultSpy).toHaveBeenCalled();
    expect(component.hideToken()).toBe(false);
  });
});
