/**
 * @file modules/purchase/bill-list/bill-list.component.ts
 * @description Displays a list of all purchase bills with actions to Create, Edit, and Export PDF.
 * Shows sync status for offline-saved bills.
 */

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../services/api.service';
import { OfflineService } from '../../../services/offline.service';
import { PdfService } from '../../../services/pdf.service';
import { PurchaseBillSummary, OfflinePurchaseBill } from '../../../models';

@Component({
  selector: 'app-bill-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './bill-list.component.html',
  styleUrls: ['./bill-list.component.scss']
})
export class BillListComponent implements OnInit {
  /** List of synced purchase bills from the server */
  bills: PurchaseBillSummary[] = [];

  /** Offline bills pending sync */
  offlineBills: OfflinePurchaseBill[] = [];

  isLoading = false;
  errorMessage = '';
  isSyncing = false;

  constructor(
    private apiService: ApiService,
    private offlineService: OfflineService,
    private pdfService: PdfService
  ) {}

  ngOnInit(): void {
    this.loadBills();
    this.loadOfflineBills();

    // Refresh offline list when sync status changes
    this.offlineService.syncStatusChanged$.subscribe(() => {
      this.loadOfflineBills();
      this.loadBills();
    });
  }

  /** Fetches all purchase bills from the server */
  loadBills(): void {
    if (!this.offlineService.isOnline()) return;

    this.isLoading = true;
    this.apiService.getPurchaseBills().subscribe({
      next: (data) => {
        this.bills = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load purchase bills. Check your connection.';
        this.isLoading = false;
      }
    });
  }

  /** Loads offline-saved bills from LocalStorage */
  loadOfflineBills(): void {
    this.offlineBills = this.offlineService.getAllOfflineBills();
  }

  /** Manual sync trigger for pending offline bills */
  syncNow(): void {
    this.isSyncing = true;
    this.offlineService.syncPending().then(() => {
      this.isSyncing = false;
      this.loadOfflineBills();
      this.loadBills();
    });
  }

  /** Exports a purchase bill to PDF by fetching its full details */
  exportPdf(id: number): void {
    this.apiService.getPurchaseBillById(id).subscribe({
      next: (bill) => this.pdfService.exportPurchaseBillToPdf(bill),
      error: () => alert('Failed to load bill for PDF export.')
    });
  }

  get isOnline(): boolean {
    return this.offlineService.isOnline();
  }

  get pendingCount(): number {
    return this.offlineBills.filter(b => b.syncStatus === 'Pending').length;
  }
}
