import { Component, inject, signal } from '@angular/core';
import { ApiService, DashboardView } from '../api.service';

/** How well the automation is working, in numbers. */
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {
  private api = inject(ApiService);

  view = signal<DashboardView | null>(null);
  loading = signal(false);
  error = signal('');

  constructor() {
    void this.refresh();
  }

  /** The self-service rate as a whole percentage. */
  selfServicePercent(): number {
    return Math.round((this.view()?.selfServiceRate ?? 0) * 100);
  }

  /** The request types, largest group first. */
  types(): { name: string; count: number }[] {
    const byType = this.view()?.byType ?? {};

    return Object.entries(byType)
      .map(([name, count]) => ({ name, count }))
      .sort((first, second) => second.count - first.count);
  }

  /** How wide a type's bar should be, relative to the largest group. */
  share(count: number): number {
    const largest = Math.max(...this.types().map((type) => type.count), 1);

    return Math.round((count / largest) * 100);
  }

  async refresh(): Promise<void> {
    this.loading.set(true);
    this.error.set('');

    try {
      this.view.set(await this.api.dashboard());
    } catch {
      this.error.set('Die Kennzahlen konnten nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }
}
