import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

/** What the API sends back after a message was classified and routed. */
export interface RequestView {
  id: number;
  message: string;
  type: string;
  confidence: number;
  urgency: string;
  reason: string;
  destination: string;
  status: string;
  decidedByHuman: boolean;
  receivedAt: string;
}

/** One place for every call to the C# API. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  /** Sends a customer message and returns how it was classified and routed. */
  send(message: string): Promise<RequestView> {
    return firstValueFrom(this.http.post<RequestView>('/api/requests', { message }));
  }

  /** The requests the classifier was not sure enough about. */
  reviewQueue(): Promise<RequestView[]> {
    return firstValueFrom(this.http.get<RequestView[]>('/api/requests/review'));
  }

  /** An employee picks the type for a request in the queue. */
  resolve(id: number, type: string): Promise<RequestView> {
    return firstValueFrom(this.http.post<RequestView>(`/api/requests/${id}/resolve`, { type }));
  }
}
