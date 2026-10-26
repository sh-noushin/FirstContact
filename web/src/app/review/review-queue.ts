import { Component, inject, signal } from '@angular/core';
import { ApiService, RequestView } from '../api.service';

/** The type an employee can choose, with the German label shown on the button. */
interface TypeChoice {
  value: string;
  label: string;
}

/** Requests the classifier was not sure about. A person decides where each one goes. */
@Component({
  selector: 'app-review-queue',
  templateUrl: './review-queue.html',
  styleUrl: './review-queue.css',
})
export class ReviewQueue {
  private api = inject(ApiService);

  readonly choices: TypeChoice[] = [
    { value: 'Question', label: 'Bedienungsfrage' },
    { value: 'Problem', label: 'Problem' },
    { value: 'FeatureRequest', label: 'Funktionswunsch' },
    { value: 'Other', label: 'Sonstiges' },
  ];

  waiting = signal<RequestView[]>([]);
  loading = signal(false);
  error = signal('');

  /** The request currently being resolved, so only its buttons are disabled. */
  busyId = signal<number | null>(null);

  constructor() {
    void this.refresh();
  }

  async refresh(): Promise<void> {
    this.loading.set(true);
    this.error.set('');

    try {
      this.waiting.set(await this.api.reviewQueue());
    } catch {
      this.error.set('Die Warteschlange konnte nicht geladen werden.');
    } finally {
      this.loading.set(false);
    }
  }

  /** Records the decision and takes the request out of the list. */
  async decide(request: RequestView, type: string): Promise<void> {
    this.busyId.set(request.id);
    this.error.set('');

    try {
      await this.api.resolve(request.id, type);
      this.waiting.update((queue) => queue.filter((item) => item.id !== request.id));
    } catch {
      this.error.set('Die Entscheidung konnte nicht gespeichert werden.');
    } finally {
      this.busyId.set(null);
    }
  }
}
