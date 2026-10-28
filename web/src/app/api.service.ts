import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

/** The answer the bot found in the handbook, with the articles it used. */
export interface AnswerView {
  answered: boolean;
  text: string;
  sources: string[];
}

/** One handbook article. */
export interface ArticleView {
  id: number;
  title: string;
  content: string;
}

/** A question the handbook could not answer. */
export interface GapView {
  id: number;
  question: string;
  timesAsked: number;
  lastAskedAt: string;
}

/** A grouped feature wish, with the customers' own words as evidence. */
export interface DemandView {
  id: number;
  title: string;
  customerCount: number;
  quotes: string[];
  lastSeen: string;
}

/** The numbers behind the automation. */
export interface DashboardView {
  totalRequests: number;
  answeredByBot: number;
  selfServiceRate: number;
  toSupport: number;
  toProduct: number;
  awaitingReview: number;
  averageConfidence: number;
  openKnowledgeGaps: number;
  byType: Record<string, number>;
}

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
  answeredByBot: boolean;
  receivedAt: string;
  answer: AnswerView | null;
}

/** One place for every call to the C# API. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  /** Sends a customer message and returns how it was classified and routed. */
  send(message: string): Promise<RequestView> {
    return firstValueFrom(this.http.post<RequestView>('/api/requests', { message }));
  }

  /** Adds a handbook article, or improves the one with the same title. */
  writeArticle(title: string, content: string): Promise<ArticleView> {
    return firstValueFrom(
      this.http.post<ArticleView>('/api/knowledge/articles', { title, content }),
    );
  }

  /** What customers are asking for, grouped by idea. */
  features(): Promise<DemandView[]> {
    return firstValueFrom(this.http.get<DemandView[]>('/api/features'));
  }

  /** The numbers for the dashboard. */
  dashboard(): Promise<DashboardView> {
    return firstValueFrom(this.http.get<DashboardView>('/api/dashboard'));
  }

  /** The questions the handbook could not answer, most asked first. */
  knowledgeGaps(): Promise<GapView[]> {
    return firstValueFrom(this.http.get<GapView[]>('/api/knowledge/gaps'));
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
