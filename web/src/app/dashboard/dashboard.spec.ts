import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ApiService, DashboardView } from '../api.service';
import { Dashboard } from './dashboard';

/** Stands in for the API, so the component test makes no HTTP call. */
class StubApi {
  numbers: DashboardView = view();
  fails = false;

  dashboard(): Promise<DashboardView> {
    return this.fails ? Promise.reject(new Error('offline')) : Promise.resolve(this.numbers);
  }
}

describe('Dashboard', () => {
  let fixture: ComponentFixture<Dashboard>;
  let page: Dashboard;
  let api: StubApi;

  beforeEach(async () => {
    api = new StubApi();

    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [{ provide: ApiService, useValue: api }],
    }).compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    page = fixture.componentInstance;

    await fixture.whenStable();
  });

  it('loads the numbers when it opens', () => {
    expect(page.view()?.totalRequests).toBe(10);
    expect(page.loading()).toBe(false);
  });

  it('shows the self-service rate as a percentage', () => {
    expect(page.selfServicePercent()).toBe(40);
  });

  it('reports zero percent when nothing has been answered', async () => {
    api.numbers = view({ totalRequests: 0, answeredByBot: 0, selfServiceRate: 0, byType: {} });
    await page.refresh();

    expect(page.selfServicePercent()).toBe(0);
  });

  it('lists the types with the largest group first', () => {
    expect(page.types().map((type) => type.name)).toEqual(['Question', 'Problem', 'FeatureRequest']);
  });

  it('sizes each bar against the largest group', () => {
    expect(page.share(5)).toBe(100);
    expect(page.share(3)).toBe(60);
  });

  it('shows an error when the numbers cannot be loaded', async () => {
    api.fails = true;

    await page.refresh();

    expect(page.error()).not.toBe('');
  });

  it('renders the headline numbers', () => {
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('40%');
    expect(text).toContain('4 von 10 ohne Mitarbeiter gelöst');
    expect(text).toContain('0.82');
  });

  it('invites the user to start when there is nothing yet', async () => {
    api.numbers = view({ totalRequests: 0, byType: {} });
    await page.refresh();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Noch keine Anfragen');
  });
});

function view(overrides: Partial<DashboardView> = {}): DashboardView {
  return {
    totalRequests: 10,
    answeredByBot: 4,
    selfServiceRate: 0.4,
    toSupport: 3,
    toProduct: 1,
    awaitingReview: 2,
    averageConfidence: 0.82,
    openKnowledgeGaps: 2,
    byType: { Question: 5, Problem: 3, FeatureRequest: 2 },
    ...overrides,
  };
}
