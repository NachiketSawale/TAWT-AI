import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  let isAuthenticatedMock: ReturnType<typeof vi.fn>;
  let createUrlTreeMock: ReturnType<typeof vi.fn>;
  let urlTree: UrlTree;

  const route = {} as ActivatedRouteSnapshot;
  const state = {} as RouterStateSnapshot;

  const runGuard = () =>
    TestBed.runInInjectionContext(() => authGuard(route, state));

  beforeEach(() => {
    isAuthenticatedMock = vi.fn();
    urlTree = {} as UrlTree;
    createUrlTreeMock = vi.fn().mockReturnValue(urlTree);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { isAuthenticated: isAuthenticatedMock } },
        { provide: Router, useValue: { createUrlTree: createUrlTreeMock } }
      ]
    });
  });

  it('allows activation when the user is authenticated', () => {
    isAuthenticatedMock.mockReturnValue(true);

    const result = runGuard();

    expect(result).toBe(true);
    expect(createUrlTreeMock).not.toHaveBeenCalled();
  });

  it('redirects to /login when the user is not authenticated', () => {
    isAuthenticatedMock.mockReturnValue(false);

    const result = runGuard();

    expect(result).toBe(urlTree);
    expect(createUrlTreeMock).toHaveBeenCalledWith(['/login']);
  });

  it('checks authentication status on every invocation', () => {
    isAuthenticatedMock.mockReturnValue(true);
    runGuard();
    isAuthenticatedMock.mockReturnValue(false);
    runGuard();

    expect(isAuthenticatedMock).toHaveBeenCalledTimes(2);
  });
});
