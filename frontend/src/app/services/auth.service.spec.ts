import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
import { PatLoginResponse } from '../models/auth.model';

const STORAGE_KEY = 'app-auth.session';
const LOGIN_URL = `${environment.apiUrl}/api/v1/Auth/login`;

describe('AuthService', () => {
  let httpMock: HttpTestingController;

  const createService = () => TestBed.inject(AuthService);

  const successResponse: PatLoginResponse = {
    success: true,
    accessToken: 'token-123',
    expiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
    displayName: 'Jane Doe',
    emailAddress: 'jane.doe@example.com',
    organization: 'ribdev',
    message: ''
  };

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  describe('initial state', () => {
    it('starts with no session when localStorage is empty', () => {
      const service = createService();
      expect(service.session()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
    });

    it('restores a valid session from localStorage on creation', () => {
      const stored = {
        accessToken: 'stored-token',
        expiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
        displayName: 'Stored User',
        emailAddress: 'stored@example.com',
        organization: 'ribdev'
      };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(stored));

      const service = createService();

      expect(service.session()).toEqual(stored);
      expect(service.isAuthenticated()).toBe(true);
    });

    it('discards and clears an expired session found in localStorage', () => {
      const expired = {
        accessToken: 'stale-token',
        expiresAtUtc: new Date(Date.now() - 1000).toISOString(),
        displayName: 'Old User',
        emailAddress: 'old@example.com',
        organization: 'ribdev'
      };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(expired));

      const service = createService();

      expect(service.session()).toBeNull();
      expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
    });

    it('discards corrupt JSON found in localStorage without throwing', () => {
      localStorage.setItem(STORAGE_KEY, '{not-valid-json');

      expect(() => createService()).not.toThrow();

      const service = createService();
      expect(service.session()).toBeNull();
      expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
    });
  });

  describe('login', () => {
    it('persists the session and updates the signal on a successful response', () => {
      const service = createService();

      service.login({ patToken: 'pat-abc' }).subscribe();

      const req = httpMock.expectOne(LOGIN_URL);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ patToken: 'pat-abc' });
      req.flush(successResponse);

      expect(service.session()).toEqual({
        accessToken: successResponse.accessToken,
        expiresAtUtc: successResponse.expiresAtUtc,
        displayName: successResponse.displayName,
        emailAddress: successResponse.emailAddress,
        organization: successResponse.organization
      });
      expect(service.isAuthenticated()).toBe(true);
      expect(JSON.parse(localStorage.getItem(STORAGE_KEY)!)).toEqual(service.session());
    });

    it('does not persist a session when the response reports failure', () => {
      const service = createService();

      service.login({ patToken: 'bad-pat' }).subscribe();

      const req = httpMock.expectOne(LOGIN_URL);
      req.flush({ success: false, message: 'Invalid token' } satisfies PatLoginResponse);

      expect(service.session()).toBeNull();
      expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
    });

    it('does not persist a session when success is true but token/expiry fields are missing', () => {
      const service = createService();

      service.login({ patToken: 'pat-abc' }).subscribe();

      const req = httpMock.expectOne(LOGIN_URL);
      req.flush({ success: true, message: '' } satisfies PatLoginResponse);

      expect(service.session()).toBeNull();
      expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
    });

    it('propagates HTTP errors to the caller without touching the session', () => {
      const service = createService();
      let caughtError: unknown;

      service.login({ patToken: 'pat-abc' }).subscribe({
        error: (err) => (caughtError = err)
      });

      const req = httpMock.expectOne(LOGIN_URL);
      req.flush({ message: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

      expect(caughtError).toBeTruthy();
      expect(service.session()).toBeNull();
    });
  });

  describe('logout', () => {
    it('clears the session signal and localStorage', () => {
      const service = createService();

      service.login({ patToken: 'pat-abc' }).subscribe();
      httpMock.expectOne(LOGIN_URL).flush(successResponse);
      expect(service.isAuthenticated()).toBe(true);

      service.logout();

      expect(service.session()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
      expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
    });
  });

  describe('isAuthenticated', () => {
    it('returns false once the session has expired', () => {
      const service = createService();

      service.login({ patToken: 'pat-abc' }).subscribe();
      httpMock.expectOne(LOGIN_URL).flush({
        ...successResponse,
        expiresAtUtc: new Date(Date.now() - 1000).toISOString()
      });

      expect(service.isAuthenticated()).toBe(false);
    });
  });
});
