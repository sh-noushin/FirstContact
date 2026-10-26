import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, RequestView } from '../api.service';

/** Send a customer message and see how it was classified and where it went. */
@Component({
  selector: 'app-request-form',
  imports: [FormsModule],
  templateUrl: './request-form.html',
  styleUrl: './request-form.css',
})
export class RequestForm {
  private api = inject(ApiService);

  readonly examples = [
    'Wie exportiere ich ein Dokument als PDF?',
    'Zertifikat ungültig, ich brauche das heute noch!',
    'Eine App fürs Handy wäre super',
    'Der Export dauert ewig, geht das schneller?',
  ];

  message = signal('');
  result = signal<RequestView | null>(null);
  loading = signal(false);
  error = signal('');

  /** German label for the request type. */
  typeLabel(type: string): string {
    switch (type) {
      case 'Question': return 'Bedienungsfrage';
      case 'Problem': return 'Problem';
      case 'FeatureRequest': return 'Funktionswunsch';
      default: return 'Sonstiges';
    }
  }

  /** German label for where the request went. */
  destinationLabel(destination: string): string {
    switch (destination) {
      case 'SelfService': return 'Self-Service';
      case 'SupportTeam': return 'Support-Team';
      case 'ProductTeam': return 'Produktmanagement';
      default: return 'Manuelle Prüfung';
    }
  }

  /** True when the classifier was not sure enough and a person has to decide. */
  needsHuman(): boolean {
    return this.result()?.destination === 'HumanReview';
  }

  use(example: string): void {
    this.message.set(example);
    void this.send();
  }

  async send(): Promise<void> {
    if (!this.message().trim() || this.loading()) {
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.result.set(null);

    try {
      this.result.set(await this.api.send(this.message()));
    } catch {
      this.error.set('Die Anfrage ist fehlgeschlagen. Läuft die API auf Port 5080?');
    } finally {
      this.loading.set(false);
    }
  }
}
