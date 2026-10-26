import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ApiService, GapView } from '../api.service';
import { KnowledgeGaps } from './knowledge-gaps';

/** Stands in for the API, so the component test makes no HTTP call. */
class StubApi {
  gaps: GapView[] = [gap(1, 'Gibt es eine Schnittstelle zu SAP?', 3), gap(2, 'Was kostet eine Lizenz?', 1)];
  fails = false;

  knowledgeGaps(): Promise<GapView[]> {
    return this.fails ? Promise.reject(new Error('offline')) : Promise.resolve([...this.gaps]);
  }
}

describe('KnowledgeGaps', () => {
  let fixture: ComponentFixture<KnowledgeGaps>;
  let page: KnowledgeGaps;
  let api: StubApi;

  beforeEach(async () => {
    api = new StubApi();

    await TestBed.configureTestingModule({
      imports: [KnowledgeGaps],
      providers: [{ provide: ApiService, useValue: api }],
    }).compileComponents();

    fixture = TestBed.createComponent(KnowledgeGaps);
    page = fixture.componentInstance;

    await fixture.whenStable();
  });

  it('loads the gaps when it opens', () => {
    expect(page.gaps()).toHaveLength(2);
    expect(page.loading()).toBe(false);
  });

  it('adds up how many customers were affected', () => {
    expect(page.totalAsked()).toBe(4);
  });

  it('shows an error when the gaps cannot be loaded', async () => {
    api.fails = true;

    await page.refresh();

    expect(page.error()).not.toBe('');
  });

  it('shows a friendly message when the handbook covered everything', async () => {
    api.gaps = [];
    await page.refresh();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Keine Lücken');
  });

  it('renders each question with how often it was asked', () => {
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Gibt es eine Schnittstelle zu SAP?');
    expect(text).toContain('3×');
    expect(text).toContain('2 offene Themen');
  });

  it('marks a frequently asked gap so it stands out', () => {
    fixture.detectChanges();

    const hot = fixture.nativeElement.querySelectorAll('.count.hot') as NodeListOf<HTMLElement>;

    expect(hot).toHaveLength(1);
    expect(hot[0].textContent).toContain('3');
  });
});

function gap(id: number, question: string, timesAsked: number): GapView {
  return { id, question, timesAsked, lastAskedAt: '2026-10-26T08:00:00+00:00' };
}
