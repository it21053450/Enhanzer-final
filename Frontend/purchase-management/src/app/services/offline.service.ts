/**
 * @file services/offline.service.ts
 * @description Offline capability service using LocalStorage (Task 5.3 - MANDATORY).
 * 
 * Features:
 *   - Save purchase bills locally when offline
 *   - Auto-sync when connection is restored
 *   - Duplicate prevention using unique offline IDs
 *   - Sync status tracking: Pending / Synced / Failed
 * 
 * Uses LocalStorage for persistence (can be upgraded to IndexedDB for larger datasets).
 */

import { Injectable, OnDestroy } from '@angular/core';
import { Subject, Subscription, fromEvent, merge } from 'rxjs';
import { ApiService } from './api.service';
import {
  OfflinePurchaseBill,
  CreatePurchaseBillRequest,
  SyncStatus
} from '../models';

const STORAGE_KEY = 'purchase_bills_offline';

@Injectable({
  providedIn: 'root'
})
export class OfflineService implements OnDestroy {
  /** Emits whenever the sync status changes */
  syncStatusChanged$ = new Subject<void>();

  private onlineSubscription: Subscription;

  constructor(private apiService: ApiService) {
    // Listen for browser online/offline events to trigger auto-sync
    this.onlineSubscription = merge(
      fromEvent(window, 'online'),
      fromEvent(window, 'offline')
    ).subscribe(() => {
      if (this.isOnline()) {
        // Auto-sync all pending records when connection is restored
        this.syncPending();
      }
    });
  }

  ngOnDestroy(): void {
    this.onlineSubscription.unsubscribe();
  }

  // ── Status Helpers ──────────────────────────────────────────────────────────

  /** Returns true if the browser is currently connected to the internet */
  isOnline(): boolean {
    return navigator.onLine;
  }

  // ── Storage Operations ──────────────────────────────────────────────────────

  /**
   * Saves a purchase bill to LocalStorage with a Pending sync status.
   * Each bill gets a unique UUID as offlineId to prevent duplicates.
   * 
   * @param data - The purchase bill data to save offline
   * @returns The newly created offline record
   */
  saveBillOffline(data: CreatePurchaseBillRequest): OfflinePurchaseBill {
    const offlineRecord: OfflinePurchaseBill = {
      offlineId: this.generateUUID(),
      data,
      syncStatus: 'Pending',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };

    const existing = this.getAllOfflineBills();
    existing.push(offlineRecord);
    this.persistToStorage(existing);

    return offlineRecord;
  }

  /** Returns all offline purchase bills from LocalStorage */
  getAllOfflineBills(): OfflinePurchaseBill[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }

  /** Returns only bills with Pending sync status */
  getPendingBills(): OfflinePurchaseBill[] {
    return this.getAllOfflineBills().filter(b => b.syncStatus === 'Pending');
  }

  /** 
   * Attempts to sync all Pending offline bills to the server.
   * Updates each record's syncStatus to 'Synced' or 'Failed'.
   * Duplicate prevention: once a bill has serverId or syncStatus='Synced', it is skipped.
   */
  async syncPending(): Promise<void> {
    const pending = this.getPendingBills();
    if (pending.length === 0) return;

    for (const record of pending) {
      try {
        // Call the API to create the bill on the server
        const response = await this.apiService.createPurchaseBill(record.data).toPromise();

        if (response?.success && response.data) {
          // Mark as synced and store the server-assigned ID
          this.updateSyncStatus(record.offlineId, 'Synced', response.data.id);
        } else {
          this.updateSyncStatus(record.offlineId, 'Failed');
        }
      } catch {
        // Network error or server rejection — mark as Failed
        this.updateSyncStatus(record.offlineId, 'Failed');
      }
    }

    // Notify subscribers (e.g., UI components showing sync status)
    this.syncStatusChanged$.next();
  }

  /**
   * Updates the sync status of a specific offline bill.
   * @param offlineId - The UUID of the offline record
   * @param status - New sync status
   * @param serverId - Server-assigned ID after successful sync
   */
  updateSyncStatus(offlineId: string, status: SyncStatus, serverId?: number): void {
    const all = this.getAllOfflineBills();
    const index = all.findIndex(b => b.offlineId === offlineId);

    if (index !== -1) {
      all[index].syncStatus = status;
      all[index].updatedAt = new Date().toISOString();
      if (serverId !== undefined) {
        all[index].serverId = serverId;
      }
      this.persistToStorage(all);
    }
  }

  /** Removes a successfully synced bill from local storage */
  removeSyncedBill(offlineId: string): void {
    const filtered = this.getAllOfflineBills().filter(b => b.offlineId !== offlineId);
    this.persistToStorage(filtered);
  }

  // ── Private Helpers ─────────────────────────────────────────────────────────

  private persistToStorage(bills: OfflinePurchaseBill[]): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(bills));
  }

  /**
   * Generates a UUID v4 for unique offline record identification.
   * Prevents duplicate entries when the same bill is edited offline.
   */
  private generateUUID(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      const r = Math.random() * 16 | 0;
      const v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }
}
