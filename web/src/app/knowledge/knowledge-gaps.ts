import { Component, inject, signal } from '@angular/core';
import { ApiService, GapView } from '../api.service';

/**
 * Questions the handbook could not answer. Each one is a candidate for a new article,
 * and the count shows how many customers ran into it.
 */
@Component({
  selector: 'app-knowledge-gaps',
  templateUrl: './knowledge-gaps.html',
  styleUrl: './knowledge-gaps.css',
})
export class KnowledgeGaps {
  private api = inject(ApiService);

  gaps = signal<GapView[]>([]);
  loading = signal(false);
  error = signal('');

  constructor() {
    void this.refresh();
  }

  /** How many customers ran into gaps in total. */
  totalAsked(): number {
    return this.gaps().reduce((sum, gap) => sum + gap.timesAsked, 0);
  }

  async refresh(): Promise<void> {
    this.loading.set(true);
    this.error.set('');

    try {
      this.gaps.set(await this.api.knowledgeGaps());
    } catch {
      this.error.set('Die Wissenslücken konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }
}
