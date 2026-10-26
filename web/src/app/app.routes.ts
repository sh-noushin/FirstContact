import { Routes } from '@angular/router';
import { KnowledgeGaps } from './knowledge/knowledge-gaps';
import { RequestForm } from './requests/request-form';
import { ReviewQueue } from './review/review-queue';

export const routes: Routes = [
  { path: '', redirectTo: 'anfrage', pathMatch: 'full' },
  { path: 'anfrage', component: RequestForm },
  { path: 'pruefung', component: ReviewQueue },
  { path: 'wissen', component: KnowledgeGaps },
];
