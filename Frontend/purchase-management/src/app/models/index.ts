/**
 * @file models/index.ts
 * @description Central export barrel for all TypeScript interfaces and types.
 * All models are strongly typed matching the backend DTOs exactly.
 */

// ── Master Data Models ──────────────────────────────────────────────────────

/** Represents a product item from GET /api/items */
export interface Item {
  id: number;
  name: string;
  isActive: boolean;
}

/** Represents a warehouse/store location from GET /api/locations */
export interface Location {
  id: number;
  code: string;
  name: string;
  isActive: boolean;
}

// ── Purchase Bill Models ────────────────────────────────────────────────────

/** A single line item row in the purchase bill form */
export interface PurchaseBillItem {
  id?: number;
  itemId: number;
  itemName?: string;
  locationId: number;
  locationCode?: string;
  locationName?: string;
  cost: number;
  price: number;
  quantity: number;
  discountPercent: number;
  totalCost: number;
  totalSelling: number;
  sortOrder?: number;
}

/** Full purchase bill object returned from API */
export interface PurchaseBill {
  id: number;
  billNumber: string;
  billDate: string;
  notes?: string;
  status: string;
  totalItems: number;
  totalQuantity: number;
  totalAmount: number;
  createdAt: string;
  updatedAt?: string;
  items: PurchaseBillItem[];
}

/** Lightweight summary for the bill list view */
export interface PurchaseBillSummary {
  id: number;
  billNumber: string;
  billDate: string;
  status: string;
  totalItems: number;
  totalQuantity: number;
  totalAmount: number;
  createdAt: string;
}

/** Payload sent to POST /api/purchase-bill */
export interface CreatePurchaseBillRequest {
  billDate: string;
  notes?: string;
  items: CreatePurchaseBillItemRequest[];
}

/** Single line item payload */
export interface CreatePurchaseBillItemRequest {
  itemId: number;
  locationId: number;
  cost: number;
  price: number;
  quantity: number;
  discountPercent: number;
  sortOrder: number;
}

/** Payload sent to PUT /api/purchase-bill/{id} */
export interface UpdatePurchaseBillRequest {
  billDate: string;
  notes?: string;
  items: CreatePurchaseBillItemRequest[];
}

// ── Audit Log Models ────────────────────────────────────────────────────────

/** Represents an audit log entry from Task 5.4 */
export interface AuditLog {
  id: number;
  entity: string;
  action: string;
  oldValue?: string;
  newValue?: string;
  entityId?: number;
  createdAt: string;
}

// ── Generic API Response ────────────────────────────────────────────────────

/** Standard API response wrapper matching backend ApiResponse<T> */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
}

// ── Offline Sync Models ─────────────────────────────────────────────────────

/** Status of an offline-saved purchase bill */
export type SyncStatus = 'Pending' | 'Synced' | 'Failed';

/** Offline purchase bill stored in IndexedDB / LocalStorage (Task 5.3) */
export interface OfflinePurchaseBill {
  /** Unique offline ID (uuid) */
  offlineId: string;
  /** The bill data (same structure as CreatePurchaseBillRequest) */
  data: CreatePurchaseBillRequest;
  /** ID from server after sync (null until synced) */
  serverId?: number;
  /** Current sync status */
  syncStatus: SyncStatus;
  /** When this offline record was created */
  createdAt: string;
  /** When this offline record was last updated */
  updatedAt: string;
}
