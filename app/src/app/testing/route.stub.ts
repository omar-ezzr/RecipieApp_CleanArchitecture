import { of } from 'rxjs';

/** Minimal route contract for standalone component unit tests. */
export const activatedRouteStub = {
  snapshot: { paramMap: { get: () => null }, queryParamMap: { get: () => null }, params: {}, queryParams: {} },
  paramMap: of({ get: () => null }),
  queryParamMap: of({ get: () => null }),
  params: of({}),
  queryParams: of({})
};
