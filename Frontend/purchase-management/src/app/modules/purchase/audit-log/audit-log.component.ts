/**
 * @file modules/purchase/audit-log/audit-log.component.ts
 * @description Displays audit trail records for Purchase Bills (Task 5.4 - MANDATORY).
 * Shows Create and Update actions with before/after JSON values.
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../services/api.service';
import { AuditLog } from '../../../models';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './audit-log.component.html',
  styleUrls: ['./audit-log.component.scss']
})
export class AuditLogComponent implements OnInit {
  logs: AuditLog[] = [];
  isLoading = false;
  errorMessage = '';

  /** Tracks which audit log rows have expanded JSON views */
  expandedRows = new Set<number>();

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading = true;
    this.apiService.getAuditLogs(100).subscribe({
      next: (data) => { this.logs = data; this.isLoading = false; },
      error: () => { this.errorMessage = 'Failed to load audit logs.'; this.isLoading = false; }
    });
  }

  /** Toggles expanded view for JSON old/new values */
  toggleExpand(id: number): void {
    this.expandedRows.has(id) ? this.expandedRows.delete(id) : this.expandedRows.add(id);
  }

  isExpanded(id: number): boolean {
    return this.expandedRows.has(id);
  }

  /** Formats JSON string for readable display */
  formatJson(json?: string): string {
    if (!json) return '—';
    try { return JSON.stringify(JSON.parse(json), null, 2); }
    catch { return json; }
  }

  getActionClass(action: string): string {
    return action.toLowerCase() === 'create' ? 'action-create' : 'action-update';
  }
}
