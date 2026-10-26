import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ApiService, RequestView } from '../api.service';
import { RequestForm } from './request-form';

/** Stands in for the API, so the component test makes no HTTP call. */
class StubApi {
  answer: RequestView = view();
  fails = false;
  sent: string[] = [];

  send(message: string): Promise<RequestView> {
    this.sent.push(message);
    return this.fails ? Promise.reject(new Error('offline')) : Promise.resolve(this.answer);
  }
}

describe('RequestForm', () => {
  let fixture: ComponentFixture<RequestForm>;
  let form: RequestForm;
  let api: StubApi;

  beforeEach(async () => {
    api = new StubApi();

    await TestBed.configureTestingModule({
      imports: [RequestForm],
      providers: [{ provide: ApiService, useValue: api }],
    }).compileComponents();

    fixture = TestBed.createComponent(RequestForm);
    form = fixture.componentInstance;
  });

  it('starts empty', () => {
    expect(form.message()).toBe('');
    expect(form.result()).toBeNull();
    expect(form.loading()).toBe(false);
  });

  it('does not call the api for an empty message', async () => {
    form.message.set('   ');
    await form.send();

    expect(api.sent).toHaveLength(0);
  });

  it('shows the result after sending', async () => {
    form.message.set('Wie exportiere ich?');
    await form.send();

    expect(api.sent).toEqual(['Wie exportiere ich?']);
    expect(form.result()?.type).toBe('Question');
    expect(form.loading()).toBe(false);
  });

  it('shows an error when the api cannot be reached', async () => {
    api.fails = true;
    form.message.set('Eine Frage');
    await form.send();

    expect(form.error()).not.toBe('');
    expect(form.result()).toBeNull();
    expect(form.loading()).toBe(false);
  });

  it('clicking an example sends it straight away', async () => {
    await form.use(form.examples[0]);

    expect(api.sent).toEqual([form.examples[0]]);
  });

  it('reports when a person has to decide', async () => {
    api.answer = view({ destination: 'HumanReview', confidence: 0.4 });
    form.message.set('Unklare Nachricht');
    await form.send();

    expect(form.needsHuman()).toBe(true);
  });

  it('does not report human review for a routed request', async () => {
    form.message.set('Eine Frage');
    await form.send();

    expect(form.needsHuman()).toBe(false);
  });

  it.each([
    ['Question', 'Bedienungsfrage'],
    ['Problem', 'Problem'],
    ['FeatureRequest', 'Funktionswunsch'],
    ['Other', 'Sonstiges'],
  ])('translates the type %s', (type, expected) => {
    expect(form.typeLabel(type)).toBe(expected);
  });

  it.each([
    ['SelfService', 'Self-Service'],
    ['SupportTeam', 'Support-Team'],
    ['ProductTeam', 'Produktmanagement'],
    ['HumanReview', 'Manuelle Prüfung'],
  ])('translates the destination %s', (destination, expected) => {
    expect(form.destinationLabel(destination)).toBe(expected);
  });

  it('renders the result in the page', async () => {
    form.message.set('Wie exportiere ich?');
    await form.send();
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Bedienungsfrage');
    expect(text).toContain('Self-Service');
    expect(text).toContain('0.95');
  });
});

function view(overrides: Partial<RequestView> = {}): RequestView {
  return {
    id: 1,
    message: 'Wie exportiere ich?',
    type: 'Question',
    confidence: 0.95,
    urgency: 'Medium',
    reason: 'Bedienungsfrage erkannt',
    destination: 'SelfService',
    status: 'Routed',
    decidedByHuman: false,
    receivedAt: '2026-10-26T08:00:00+00:00',
    ...overrides,
  };
}
