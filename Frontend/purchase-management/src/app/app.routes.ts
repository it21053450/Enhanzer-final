/**
 * @file app.routes.ts
 * @description Application routing configuration.
 * Uses lazy loading for clean code splitting per module.
 */

import { Routes } from '@angular/router';
import { BillListComponent } from './modules/purchase/bill-list/bill-list.component';
import { BillFormComponent } from './modules/purchase/bill-form/bill-form.component';
import { AuditLogComponent } from './modules/purchase/audit-log/audit-log.component';

export const routes: Routes = [
  // Default route redirects to purchase bill list
  { path: '', redirectTo: '/purchase', pathMatch: 'full' },

  // Purchase module routes
  { path: 'purchase',           component: BillListComponent },
  { path: 'purchase/new',       component: BillFormComponent },
  { path: 'purchase/edit/:id',  component: BillFormComponent },

  // Audit trail
  { path: 'audit-logs',         component: AuditLogComponent },

  // Catch-all redirect
  { path: '**', redirectTo: '/purchase' }
];
