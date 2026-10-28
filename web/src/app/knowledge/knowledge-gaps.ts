import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, GapView } from '../api.service';

/**
 * Questions the handbook could not answer. Each one is a candidate for a new article,
 * and the count shows how many customers ran into it.
 */
@Component({
  selector: 'app-knowledge-gaps',
  imports: [FormsModule],
  templateUrl: './knowledge-gaps.html',
  styleUrl: './knowledge-gaps.css',
})
export class KnowledgeGaps {
  private api = inject(ApiService);

  gaps = signal<GapView[]>([]);
  loading = signal(false);
  error = signal('');

  // --- writing the missing article ---
  title = signal('');
  content = signal('');
  saving = signal(false);
  saved = signal('');

  constructor() {
    void this.refresh();
  }

  /** How many customers ran into gaps in total. */
  totalAsked(): number {
    return this.gaps().reduce((sum, gap) => sum + gap.timesAsked, 0);
  }

  /** Starts an article from a gap, so the question becomes the title to work from. */
  writeFor(gap: GapView): void {
    this.title.set(gap.question);
    this.content.set('');
    this.saved.set('');
  }

  /** Saves the article, then reloads the gaps so a solved one can disappear. */
  async save(): Promise<void> {
    if (!this.title().trim() || !this.content().trim() || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.saved.set('');

    try {
      const article = await this.api.writeArticle(this.title(), this.content());
      this.saved.set(article.title);
      this.title.set('');
      this.content.set('');
    } catch {
      this.error.set('Der Artikel konnte nicht gespeichert werden.');
    } finally {
      this.saving.set(false);
    }
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
