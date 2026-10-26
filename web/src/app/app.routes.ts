import { Routes } from '@angular/router';
import { Dashboard } from './dashboard/dashboard';
import { FeatureDemand } from './features/feature-demand';
import { KnowledgeGaps } from './knowledge/knowledge-gaps';
import { RequestForm } from './requests/request-form';
import { ReviewQueue } from './review/review-queue';

export const routes: Routes = [
  { path: '', redirectTo: 'anfrage', pathMatch: 'full' },
  { path: 'anfrage', component: RequestForm },
  { path: 'pruefung', component: ReviewQueue },
  { path: 'wuensche', component: FeatureDemand },
  { path: 'wissen', component: KnowledgeGaps },
  { path: 'kennzahlen', component: Dashboard },
];
