import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard').then(m => m.Dashboard)
  },
  {
    path: 'batches/new',
    loadComponent: () => import('./features/batches/create/create-batch').then(m => m.CreateBatch)
  },
  {
    path: 'batches/:id',
    loadComponent: () => import('./features/batches/details/batch-details').then(m => m.BatchDetails)
  },
  {
    path: 'batches',
    loadComponent: () => import('./features/batches/batches').then(m => m.Batches)
  },
  {
    path: 'work-orders',
    loadComponent: () => import('./features/work-orders/work-orders').then(m => m.WorkOrders)
  },
  { path: '**', redirectTo: 'dashboard' }
];
