import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApiService, RequestView } from './api.service';

describe('ApiService', () => {
  let api: ApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    api = TestBed.inject(ApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('posts the message to the api', async () => {
    const pending = api.send('Wie exportiere ich?');

    const call = http.expectOne('/api/requests');
    expect(call.request.method).toBe('POST');
    expect(call.request.body).toEqual({ message: 'Wie exportiere ich?' });

    call.flush(view());
    await expect(pending).resolves.toMatchObject({ type: 'Question' });
  });

  it('passes the answer through unchanged', async () => {
    const pending = api.send('Eine Frage');
    http.expectOne('/api/requests').flush(view({ destination: 'HumanReview', confidence: 0.4 }));

    const answer = await pending;
    expect(answer.destination).toBe('HumanReview');
    expect(answer.confidence).toBe(0.4);
  });
});

function view(overrides: Partial<RequestView> = {}): RequestView {
  return {
    id: 1,
    message: 'Wie exportiere ich?',
    type: 'Question',
    confidence: 0.95,
    urgency: 'Medium',
    reason: 'Bedienungsfrage',
    destination: 'SelfService',
    status: 'Routed',
    decidedByHuman: false,
    receivedAt: '2026-10-26T08:00:00+00:00',
    ...overrides,
  };
}
