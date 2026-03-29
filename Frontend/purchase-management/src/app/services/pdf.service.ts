/**
 * @file services/pdf.service.ts
 * @description PDF export service for Purchase Bills (Task 5.1 - MANDATORY).
 * 
 * Uses jsPDF + jsPDF-autotable to generate a formatted PDF document.
 * The PDF includes:
 *   - Header section (Bill Number, Date, Status)
 *   - Line items table (Item, Batch, Cost, Price, Qty, Discount%, Total Cost, Total Selling)
 *   - Summary section (Total Items, Total Quantity, Total Amount)
 */

import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { PurchaseBill } from '../models';

@Injectable({
  providedIn: 'root'
})
export class PdfService {

  /**
   * Generates and downloads a Purchase Bill PDF.
   * @param bill - The full purchase bill data (header + items)
   */
  exportPurchaseBillToPdf(bill: PurchaseBill): void {
    const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' });

    // ── Company Header ────────────────────────────────────────────────────────
    doc.setFontSize(18);
    doc.setTextColor(30, 64, 175); // Blue color
    doc.text('Purchase Management System', 14, 15);

    // ── Bill Header Section ───────────────────────────────────────────────────
    doc.setFontSize(11);
    doc.setTextColor(0, 0, 0);
    doc.text('PURCHASE BILL', 14, 25);

    doc.setFontSize(9);
    doc.setTextColor(80, 80, 80);

    const headerY = 32;
    doc.text(`Bill Number : ${bill.billNumber}`, 14, headerY);
    doc.text(`Bill Date   : ${new Date(bill.billDate).toLocaleDateString()}`, 14, headerY + 6);
    doc.text(`Status      : ${bill.status}`, 14, headerY + 12);

    if (bill.notes) {
      doc.text(`Notes       : ${bill.notes}`, 14, headerY + 18);
    }

    doc.text(`Generated   : ${new Date().toLocaleString()}`, 200, headerY, { align: 'right' });

    // ── Divider Line ──────────────────────────────────────────────────────────
    doc.setDrawColor(200, 200, 200);
    doc.line(14, headerY + 22, 283, headerY + 22);

    // ── Line Items Table ──────────────────────────────────────────────────────
    const tableStartY = headerY + 26;

    const tableColumns = [
      { header: '#',            dataKey: 'no' },
      { header: 'Item',         dataKey: 'item' },
      { header: 'Batch',        dataKey: 'batch' },
      { header: 'Cost',         dataKey: 'cost' },
      { header: 'Price',        dataKey: 'price' },
      { header: 'Qty',          dataKey: 'qty' },
      { header: 'Disc %',       dataKey: 'discount' },
      { header: 'Total Cost',   dataKey: 'totalCost' },
      { header: 'Total Selling',dataKey: 'totalSelling' }
    ];

    const tableRows = bill.items.map((item, index) => ({
      no: index + 1,
      item: item.itemName || '',
      batch: `${item.locationCode} – ${item.locationName}`,
      cost: item.cost.toFixed(2),
      price: item.price.toFixed(2),
      qty: item.quantity.toFixed(2),
      discount: `${item.discountPercent}%`,
      totalCost: item.totalCost.toFixed(2),
      totalSelling: item.totalSelling.toFixed(2)
    }));

    autoTable(doc, {
      startY: tableStartY,
      head: [tableColumns.map(c => c.header)],
      body: tableRows.map(row => tableColumns.map(c => (row as any)[c.dataKey])),
      styles: { fontSize: 8, cellPadding: 2 },
      headStyles: { fillColor: [30, 64, 175], textColor: 255, fontStyle: 'bold' },
      alternateRowStyles: { fillColor: [245, 247, 255] },
      columnStyles: {
        0: { halign: 'center', cellWidth: 8 },
        3: { halign: 'right' },
        4: { halign: 'right' },
        5: { halign: 'right' },
        6: { halign: 'center' },
        7: { halign: 'right' },
        8: { halign: 'right' }
      },
      margin: { left: 14, right: 14 }
    });

    // ── Summary Section ───────────────────────────────────────────────────────
    const finalY = (doc as any).lastAutoTable.finalY + 8;

    doc.setDrawColor(200, 200, 200);
    doc.line(14, finalY, 283, finalY);

    doc.setFontSize(9);
    doc.setTextColor(0, 0, 0);

    doc.text('SUMMARY', 14, finalY + 6);
    doc.setFontSize(8);
    doc.setTextColor(80, 80, 80);

    doc.text(`Total Items    : ${bill.totalItems}`, 14, finalY + 12);
    doc.text(`Total Quantity : ${bill.totalQuantity.toFixed(2)}`, 14, finalY + 18);

    // Total Amount in large text
    doc.setFontSize(10);
    doc.setTextColor(30, 64, 175);
    doc.setFont('helvetica', 'bold');
    doc.text(`Total Amount : ${bill.totalAmount.toFixed(2)}`, 200, finalY + 18, { align: 'right' });

    // ── Save the PDF ──────────────────────────────────────────────────────────
    const fileName = `PurchaseBill_${bill.billNumber}_${new Date().getTime()}.pdf`;
    doc.save(fileName);
  }
}
