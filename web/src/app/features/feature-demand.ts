import { Component, inject, signal } from '@angular/core';
import { ApiService, DemandView } from '../api.service';

/**
 * What customers are asking for, grouped by idea. Product management sees demand per
 * idea instead of a pile of separate messages.
 */
@Component({
  selector: 'app-feature-demand',
  templateUrl: './feature-demand.html',
  styleUrl: './feature-demand.css',
})
export class FeatureDemand {
  private api = inject(ApiService);

  demands = signal<DemandView[]>([]);
  loading = signal(false);
  error = signal('');

  /** Which groups are showing their quotes. */
  private opened = signal<Set<number>>(new Set());

  /** How many customer wishes are behind all the groups together. */
  totalWishes(): number {
    return this.demands().reduce((sum, demand) => sum + demand.customerCount, 0);
  }

  /** How wide a group's bar should be, relative to the most wanted one. */
  share(count: number): number {
    const largest = Math.max(...this.demands().map((demand) => demand.customerCount), 1);

    return Math.round((count / largest) * 100);
  }

  isOpen(id: number): boolean {
    return this.opened().has(id);
  }

  toggle(id: number): void {
    this.opened.update((open) => {
      const next = new Set(open);
      next.has(id) ? next.delete(id) : next.add(id);

      return next;
    });
  }

  async refresh(): Promise<void> {
    this.loading.set(true);
    this.error.set('');

    try {
      this.demands.set(await this.api.features());
    } catch {
      this.error.set('Die Funktionswünsche konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }

  constructor() {
    void this.refresh();
  }
}
