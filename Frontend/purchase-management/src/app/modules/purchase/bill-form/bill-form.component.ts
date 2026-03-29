/**
 * @file modules/purchase/bill-form/bill-form.component.ts
 * @description Purchase Bill create/edit form (Tasks 3, 4, 5.1, 5.2, 5.3).
 *
 * Features:
 *   - Reactive Form with dynamic line items (FormArray)
 *   - Item autocomplete with debounce
 *   - Batch/Location dropdown
 *   - Real-time calculations per row (TotalCost, TotalSelling)
 *   - Summary panel (Total Items, Total Quantity, Total Amount)
 *   - Tab navigation (Header / Details / Summary)
 *   - PDF export via PdfService
 *   - Offline save via OfflineService when backend is unreachable
 *   - Edit mode: loads existing bill and pre-fills all fields
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  FormArray,
  Validators,
  AbstractControl,
  ReactiveFormsModule
} from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { ApiService } from '../../../services/api.service';
import { OfflineService } from '../../../services/offline.service';
import { PdfService } from '../../../services/pdf.service';
import { Item, Location, PurchaseBill } from '../../../models';

/** Tab identifiers for the ERP-style tab navigation */
type ActiveTab = 'header' | 'details' | 'summary';

@Component({
  selector: 'app-bill-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './bill-form.component.html',
  styleUrls: ['./bill-form.component.scss']
})
export class BillFormComponent implements OnInit, OnDestroy {
  /** The main reactive form */
  form!: FormGroup;

  /** Currently active tab */
  activeTab: ActiveTab = 'details';

  /** Available items for autocomplete */
  items: Item[] = [];

  /** Filtered items per row (indexed by row index) */
  filteredItemsMap: { [key: number]: Item[] } = {};

  /** Tracks which autocomplete dropdown is open */
  openAutocompleteIndex = -1;

  /** All locations for the Batch dropdown */
  locations: Location[] = [];

  /** ID of the bill being edited (null = create mode) */
  editBillId: number | null = null;

  /** Bill number assigned by server (shown in header after save) */
  savedBillNumber = '';

  isLoading = false;
  isSaving = false;
  isLoadingBill = false;
  successMessage = '';
  errorMessage = '';

  /** Full bill data loaded for edit mode (used for PDF export) */
  loadedBill: PurchaseBill | null = null;

  /** Destroy notifier for subscription cleanup */
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private offlineService: OfflineService,
    private pdfService: PdfService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadMasterData();

    // Determine create vs. edit mode from route param
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editBillId = +id;
      this.loadBillForEdit(this.editBillId);
    } else {
      // New bill: start with one empty row
      this.addRow();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Form Setup ──────────────────────────────────────────────────────────────

  /** Initializes the reactive form with header fields */
  private initForm(): void {
    this.form = this.fb.group({
      billDate: [new Date().toISOString().substring(0, 10), Validators.required],
      notes: [''],
      items: this.fb.array([])
    });
  }

  // ── Master Data Loading ─────────────────────────────────────────────────────

  private loadMasterData(): void {
    this.isLoading = true;

    // Load items for autocomplete
    this.apiService.getItems()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => { this.items = data; this.isLoading = false; },
        error: () => { this.errorMessage = 'Failed to load items.'; this.isLoading = false; }
      });

    // Load locations for Batch dropdown
    this.apiService.getLocations()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => this.locations = data,
        error: () => this.errorMessage = 'Failed to load locations.'
      });
  }

  // ── Edit Mode ───────────────────────────────────────────────────────────────

  /** Loads an existing purchase bill and pre-fills the form for editing (Task 5.2) */
  private loadBillForEdit(id: number): void {
    this.isLoadingBill = true;
    this.apiService.getPurchaseBillById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (bill) => {
          this.loadedBill = bill;
          this.savedBillNumber = bill.billNumber;

          // Fill header fields
          this.form.patchValue({
            billDate: new Date(bill.billDate).toISOString().substring(0, 10),
            notes: bill.notes || ''
          });

          // Fill line items
          bill.items.forEach(item => {
            this.addRow({
              itemId: item.itemId,
              itemName: item.itemName,
              locationId: item.locationId,
              cost: item.cost,
              price: item.price,
              quantity: item.quantity,
              discountPercent: item.discountPercent
            });
          });

          this.isLoadingBill = false;
        },
        error: () => {
          this.errorMessage = 'Failed to load purchase bill for editing.';
          this.isLoadingBill = false;
        }
      });
  }

  // ── FormArray (Line Items) ──────────────────────────────────────────────────

  /** Getter for the items FormArray */
  get itemsArray(): FormArray {
    return this.form.get('items') as FormArray;
  }

  /**
   * Creates a new FormGroup for a line item row.
   * Subscribes to value changes for real-time calculation.
   */
  createRow(defaults?: Partial<{
    itemId: number; itemName: string; locationId: number;
    cost: number; price: number; quantity: number; discountPercent: number;
  }>): FormGroup {
    const group = this.fb.group({
      itemId:          [defaults?.itemId || null, Validators.required],
      itemName:        [defaults?.itemName || ''],
      locationId:      [defaults?.locationId || null, Validators.required],
      cost:            [defaults?.cost || 0, [Validators.required, Validators.min(0)]],
      price:           [defaults?.price || 0, [Validators.required, Validators.min(0)]],
      quantity:        [defaults?.quantity || 1, [Validators.required, Validators.min(0.01)]],
      discountPercent: [defaults?.discountPercent || 0, [Validators.min(0), Validators.max(100)]],
      totalCost:       [{ value: 0, disabled: true }],
      totalSelling:    [{ value: 0, disabled: true }]
    });

    // Subscribe to changes in pricing fields to trigger real-time calculations
    group.valueChanges
      .pipe(takeUntil(this.destroy$), debounceTime(100), distinctUntilChanged())
      .subscribe(() => this.recalculateRow(group));

    // Force initial calculation if defaults provided
    if (defaults) setTimeout(() => this.recalculateRow(group), 0);

    return group;
  }

  /** Adds a new empty (or pre-filled) row to the items FormArray */
  addRow(defaults?: Parameters<typeof this.createRow>[0]): void {
    this.itemsArray.push(this.createRow(defaults));
  }

  /** Removes a row from the items FormArray */
  removeRow(index: number): void {
    this.itemsArray.removeAt(index);
    delete this.filteredItemsMap[index];
  }

  // ── Calculations ─────────────────────────────────────────────────────────────

  /**
   * Recalculates TotalCost and TotalSelling for a single row.
   * TotalCost    = (Cost × Qty) - (Cost × Qty × Discount% / 100)
   * TotalSelling = Price × Qty
   */
  recalculateRow(group: AbstractControl): void {
    const cost     = +(group.get('cost')?.value || 0);
    const price    = +(group.get('price')?.value || 0);
    const qty      = +(group.get('quantity')?.value || 0);
    const disc     = +(group.get('discountPercent')?.value || 0);

    const grossCost = cost * qty;
    const totalCost = grossCost - (grossCost * disc / 100);
    const totalSelling = price * qty;

    group.get('totalCost')?.setValue(totalCost.toFixed(2), { emitEvent: false });
    group.get('totalSelling')?.setValue(totalSelling.toFixed(2), { emitEvent: false });
  }

  // ── Summary Computed Properties ──────────────────────────────────────────────

  /** Total Items = count of rows */
  get totalItems(): number {
    return this.itemsArray.length;
  }

  /** Total Quantity = SUM of all quantity values */
  get totalQuantity(): number {
    return this.itemsArray.controls.reduce((sum, g) => sum + (+(g.get('quantity')?.value || 0)), 0);
  }

  /** Total Amount = SUM of all TotalSelling values */
  get totalAmount(): number {
    return this.itemsArray.controls.reduce((sum, g) => sum + (+(g.get('totalSelling')?.value || 0)), 0);
  }

  // ── Autocomplete ──────────────────────────────────────────────────────────────

  /** Filters items based on typed text for autocomplete */
  onItemSearch(event: Event, rowIndex: number): void {
    const query = (event.target as HTMLInputElement).value.toLowerCase();
    this.openAutocompleteIndex = rowIndex;

    this.filteredItemsMap[rowIndex] = query.length > 0
      ? this.items.filter(i => i.name.toLowerCase().includes(query))
      : this.items;
  }

  /** Selects an item from the autocomplete dropdown */
  selectItem(item: Item, rowIndex: number): void {
    const group = this.itemsArray.at(rowIndex);
    group.patchValue({ itemId: item.id, itemName: item.name });
    this.openAutocompleteIndex = -1;
    this.filteredItemsMap[rowIndex] = [];
  }

  closeAutocomplete(): void {
    setTimeout(() => { this.openAutocompleteIndex = -1; }, 200);
  }

  // ── Save / Submit ─────────────────────────────────────────────────────────────

  /** Handles form submission - routes to create or update based on mode */
  onSave(): void {
    if (this.form.invalid || this.itemsArray.length === 0) {
      this.errorMessage = 'Please fill all required fields and add at least one item.';
      return;
    }

    const payload = this.buildPayload();

    if (this.editBillId) {
      this.updateBill(payload);
    } else {
      this.createBill(payload);
    }
  }

  /** Creates a new purchase bill - falls back to offline save if no connection */
  private createBill(payload: any): void {
    this.isSaving = true;
    this.errorMessage = '';

    if (!this.offlineService.isOnline()) {
      // Offline mode: save to LocalStorage (Task 5.3)
      const offlineRecord = this.offlineService.saveBillOffline(payload);
      this.successMessage = `Saved offline (ID: ${offlineRecord.offlineId.substring(0, 8)}...). Will sync when online.`;
      this.isSaving = false;
      return;
    }

    this.apiService.createPurchaseBill(payload).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.savedBillNumber = res.data.billNumber;
          this.successMessage = `Purchase Bill ${res.data.billNumber} created successfully!`;
          this.isSaving = false;
          setTimeout(() => this.router.navigate(['/purchase']), 2000);
        } else {
          this.errorMessage = res.message || 'Failed to create bill.';
          this.isSaving = false;
        }
      },
      error: () => {
        // Server unreachable - save offline
        const offlineRecord = this.offlineService.saveBillOffline(payload);
        this.successMessage = `Server unavailable. Saved offline (ID: ${offlineRecord.offlineId.substring(0, 8)}...).`;
        this.isSaving = false;
      }
    });
  }

  /** Updates an existing purchase bill (Task 5.2) */
  private updateBill(payload: any): void {
    this.isSaving = true;
    this.errorMessage = '';

    this.apiService.updatePurchaseBill(this.editBillId!, payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.successMessage = 'Purchase Bill updated successfully!';
          this.isSaving = false;
          setTimeout(() => this.router.navigate(['/purchase']), 2000);
        } else {
          this.errorMessage = res.message || 'Failed to update bill.';
          this.isSaving = false;
        }
      },
      error: () => {
        this.errorMessage = 'Failed to update bill. Please check your connection.';
        this.isSaving = false;
      }
    });
  }

  /** Exports the currently loaded (edit mode) bill to PDF (Task 5.1) */
  exportPdf(): void {
    if (this.loadedBill) {
      this.pdfService.exportPurchaseBillToPdf(this.loadedBill);
    } else if (this.editBillId) {
      this.apiService.getPurchaseBillById(this.editBillId).subscribe(bill => {
        this.pdfService.exportPurchaseBillToPdf(bill);
      });
    }
  }

  // ── Helpers ───────────────────────────────────────────────────────────────────

  private buildPayload() {
    const formValue = this.form.getRawValue();
    return {
      billDate: new Date(formValue.billDate).toISOString(),
      notes: formValue.notes || null,
      items: formValue.items.map((item: any, index: number) => ({
        itemId: item.itemId,
        locationId: item.locationId,
        cost: +item.cost,
        price: +item.price,
        quantity: +item.quantity,
        discountPercent: +item.discountPercent,
        sortOrder: index + 1
      }))
    };
  }

  setActiveTab(tab: ActiveTab): void {
    this.activeTab = tab;
  }

  getLocationName(id: number): string {
    const loc = this.locations.find(l => l.id === id);
    return loc ? `${loc.code} – ${loc.name}` : '';
  }

  get isEditMode(): boolean { return this.editBillId !== null; }
  get isOnline(): boolean   { return this.offlineService.isOnline(); }
}
