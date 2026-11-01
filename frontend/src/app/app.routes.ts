import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard').then(m => m.Dashboard)
  },
  {
    path: 'batches/new',
    canActivate: [authGuard],
    loadComponent: () => import('./features/batches/create/create-batch').then(m => m.CreateBatch)
  },
  {
    path: 'batches/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/batches/details/batch-details').then(m => m.BatchDetails)
  },
  {
    path: 'batches',
    canActivate: [authGuard],
    loadComponent: () => import('./features/batches/batches').then(m => m.Batches)
  },
  {
    path: 'work-orders',
    canActivate: [authGuard],
    loadComponent: () => import('./features/work-orders/work-orders').then(m => m.WorkOrders)
  },
  {
    path: 'mcp',
    canActivate: [authGuard],
    loadComponent: () => import('./features/mcp-playground/mcp-playground').then(m => m.McpPlayground)
  },
  { path: '**', redirectTo: 'dashboard' }
];
