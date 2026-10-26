import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ApiService, DemandView } from '../api.service';
import { FeatureDemand } from './feature-demand';

/** Stands in for the API, so the component test makes no HTTP call. */
class StubApi {
  demands: DemandView[] = [
    demand(1, 'Eine App fuers Handy waere super', 3, ['Eine App waere super', 'Mobile Anwendung bitte']),
    demand(2, 'Export nach Excel', 1, ['Export nach Excel']),
  ];
  fails = false;

  features(): Promise<DemandView[]> {
    return this.fails ? Promise.reject(new Error('offline')) : Promise.resolve([...this.demands]);
  }
}

describe('FeatureDemand', () => {
  let fixture: ComponentFixture<FeatureDemand>;
  let page: FeatureDemand;
  let api: StubApi;

  beforeEach(async () => {
    api = new StubApi();

    await TestBed.configureTestingModule({
      imports: [FeatureDemand],
      providers: [{ provide: ApiService, useValue: api }],
    }).compileComponents();

    fixture = TestBed.createComponent(FeatureDemand);
    page = fixture.componentInstance;

    await fixture.whenStable();
  });

  it('loads the groups when it opens', () => {
    expect(page.demands()).toHaveLength(2);
    expect(page.loading()).toBe(false);
  });

  it('adds up how many customers asked for something', () => {
    expect(page.totalWishes()).toBe(4);
  });

  it('sizes each bar against the most wanted idea', () => {
    expect(page.share(3)).toBe(100);
    expect(page.share(1)).toBe(33);
  });

  it('hides the quotes until they are asked for', () => {
    expect(page.isOpen(1)).toBe(false);

    page.toggle(1);
    expect(page.isOpen(1)).toBe(true);

    page.toggle(1);
    expect(page.isOpen(1)).toBe(false);
  });

  it('opens each group on its own', () => {
    page.toggle(1);

    expect(page.isOpen(1)).toBe(true);
    expect(page.isOpen(2)).toBe(false);
  });

  it('shows an error when the groups cannot be loaded', async () => {
    api.fails = true;

    await page.refresh();

    expect(page.error()).not.toBe('');
  });

  it('renders each group with its count', () => {
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Eine App fuers Handy waere super');
    expect(text).toContain('2 Themen');
    expect(text).toContain('4 Kundenwünsche');
  });

  it('shows the quotes once a group is opened', () => {
    page.toggle(1);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Mobile Anwendung bitte');
  });

  it('says so when nothing has come in yet', async () => {
    api.demands = [];
    await page.refresh();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Noch keine Funktionswünsche');
  });
});

function demand(id: number, title: string, customerCount: number, quotes: string[]): DemandView {
  return { id, title, customerCount, quotes, lastSeen: '2026-10-26T08:00:00+00:00' };
}
