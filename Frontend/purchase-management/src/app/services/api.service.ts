/**
 * @file services/api.service.ts
 * @description Core HTTP service that communicates with the ASP.NET Core backend.
 * Wraps all API calls for Items, Locations, Purchase Bills, and Audit Logs.
 */

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Item,
  Location,
  PurchaseBill,
  PurchaseBillSummary,
  CreatePurchaseBillRequest,
  UpdatePurchaseBillRequest,
  ApiResponse,
  AuditLog
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  /** Base URL — relative path so Angular dev-server proxy forwards /api/* to the backend. */
  private readonly baseUrl = '/api';

  constructor(private http: HttpClient) {}

  // ── Items (Task 2) ──────────────────────────────────────────────────────────

  /** Returns all active items for the autocomplete field */
  getItems(): Observable<Item[]> {
    return this.http.get<Item[]>(`${this.baseUrl}/items`);
  }

  /**
   * Searches items by name for autocomplete functionality.
   * @param query - Partial name to search for
   */
  searchItems(query: string): Observable<Item[]> {
    const params = new HttpParams().set('q', query);
    return this.http.get<Item[]>(`${this.baseUrl}/items/search`, { params });
  }

  // ── Locations (Task 2) ──────────────────────────────────────────────────────

  /** Returns all active locations for the Batch dropdown */
  getLocations(): Observable<Location[]> {
    return this.http.get<Location[]>(`${this.baseUrl}/locations`);
  }

  // ── Purchase Bills (Tasks 3, 4, 5.2) ───────────────────────────────────────

  /** Returns a summary list of all purchase bills */
  getPurchaseBills(): Observable<PurchaseBillSummary[]> {
    return this.http.get<PurchaseBillSummary[]>(`${this.baseUrl}/purchase-bill`);
  }

  /**
   * Returns the full detail of a purchase bill including all line items.
   * Used for Edit mode (Task 5.2) and PDF generation (Task 5.1).
   */

  getPurchaseBillById(id: number): Observable<PurchaseBill> {
    return this.http.get<PurchaseBill>(`${this.baseUrl}/purchase-bill/${id}`);
  }

  /**
   * Creates a new purchase bill on the server (Task 4).
   * @param request - Bill header and line items
   */
  createPurchaseBill(request: CreatePurchaseBillRequest): Observable<ApiResponse<PurchaseBill>> {
    return this.http.post<ApiResponse<PurchaseBill>>(`${this.baseUrl}/purchase-bill`, request);
  }

  /**
   * Updates an existing purchase bill (Task 5.2).
   * @param id - ID of the bill to update
   * @param request - Updated header and line items
   */
  updatePurchaseBill(id: number, request: UpdatePurchaseBillRequest): Observable<ApiResponse<PurchaseBill>> {
    return this.http.put<ApiResponse<PurchaseBill>>(`${this.baseUrl}/purchase-bill/${id}`, request);
  }

  // ── Audit Logs (Task 5.4) ──────────────────────────────────────────────────

  /**
   * Returns the most recent audit log entries.
   * @param pageSize - Maximum number of records to return (default: 50)
   */
  getAuditLogs(pageSize: number = 50): Observable<AuditLog[]> {
    const params = new HttpParams().set('pageSize', pageSize);
    return this.http.get<AuditLog[]>(`${this.baseUrl}/auditlogs`, { params });
  }

  /**
   * Returns audit logs filtered by entity name.
   * @param entity - Entity name (e.g., 'PurchaseBill')
   * @param entityId - Optional specific record ID
   */
  getAuditLogsByEntity(entity: string, entityId?: number): Observable<AuditLog[]> {
    let params = new HttpParams();
    if (entityId !== undefined) {
      params = params.set('entityId', entityId);
    }
    return this.http.get<AuditLog[]>(`${this.baseUrl}/auditlogs/${entity}`, { params });
  }
}
