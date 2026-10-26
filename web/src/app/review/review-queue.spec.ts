import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ApiService, RequestView } from '../api.service';
import { ReviewQueue } from './review-queue';

/** Stands in for the API, so the component test makes no HTTP call. */
class StubApi {
  queue: RequestView[] = [waiting(1, 0.4), waiting(2, 0.2)];
  queueFails = false;
  resolveFails = false;
  resolved: { id: number; type: string }[] = [];

  reviewQueue(): Promise<RequestView[]> {
    return this.queueFails
      ? Promise.reject(new Error('offline'))
      : Promise.resolve([...this.queue]);
  }

  resolve(id: number, type: string): Promise<RequestView> {
    this.resolved.push({ id, type });

    return this.resolveFails
      ? Promise.reject(new Error('offline'))
      : Promise.resolve(waiting(id, 0.4));
  }
}

describe('ReviewQueue', () => {
  let fixture: ComponentFixture<ReviewQueue>;
  let queue: ReviewQueue;
  let api: StubApi;

  beforeEach(async () => {
    api = new StubApi();

    await TestBed.configureTestingModule({
      imports: [ReviewQueue],
      providers: [{ provide: ApiService, useValue: api }],
    }).compileComponents();

    fixture = TestBed.createComponent(ReviewQueue);
    queue = fixture.componentInstance;

    await fixture.whenStable();
  });

  it('loads the queue when it opens', () => {
    expect(queue.waiting()).toHaveLength(2);
    expect(queue.loading()).toBe(false);
  });

  it('offers all four types', () => {
    expect(queue.choices.map((choice) => choice.value)).toEqual([
      'Question',
      'Problem',
      'FeatureRequest',
      'Other',
    ]);
  });

  it('sends the chosen type and removes the request from the list', async () => {
    await queue.decide(queue.waiting()[0], 'Problem');

    expect(api.resolved).toEqual([{ id: 1, type: 'Problem' }]);
    expect(queue.waiting().map((item) => item.id)).toEqual([2]);
    expect(queue.busyId()).toBeNull();
  });

  it('keeps the request in the list when saving fails', async () => {
    api.resolveFails = true;

    await queue.decide(queue.waiting()[0], 'Problem');

    expect(queue.waiting()).toHaveLength(2);
    expect(queue.error()).not.toBe('');
    expect(queue.busyId()).toBeNull();
  });

  it('shows an error when the queue cannot be loaded', async () => {
    api.queueFails = true;

    await queue.refresh();

    expect(queue.error()).not.toBe('');
  });

  it('shows a friendly message when nothing is waiting', async () => {
    api.queue = [];
    await queue.refresh();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Nichts zu prüfen');
  });

  it('renders each waiting request with its confidence', () => {
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Geht das auch anders?');
    expect(text).toContain('0.40');
    expect(text).toContain('0.20');
  });

  it('shows a button for every type', () => {
    fixture.detectChanges();

    const labels = Array.from(
      fixture.nativeElement.querySelectorAll('.choices button') as NodeListOf<HTMLButtonElement>,
    ).map((button) => button.textContent?.trim());

    expect(labels).toContain('Bedienungsfrage');
    expect(labels).toContain('Funktionswunsch');
  });
});

function waiting(id: number, confidence: number): RequestView {
  return {
    id,
    message: 'Geht das auch anders?',
    type: 'Question',
    confidence,
    urgency: 'Medium',
    reason: 'Die Nachricht ist zu vage.',
    destination: 'HumanReview',
    status: 'AwaitingReview',
    decidedByHuman: false,
    receivedAt: '2026-10-26T08:00:00+00:00',
    answer: null,
  };
}
