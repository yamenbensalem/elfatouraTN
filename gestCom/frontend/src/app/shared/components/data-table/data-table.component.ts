import { Component, Input, Output, EventEmitter, ViewChild, AfterViewInit, OnChanges, SimpleChanges } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';

export interface TableColumn {
  key: string;
  header: string;
  type?: 'text' | 'number' | 'date' | 'currency' | 'boolean';
  sortable?: boolean;
}

@Component({
  selector: 'app-data-table',
  standalone: false,
  template: `
    <div class="table-container">
      <!-- Loading overlay -->
      <div class="loading-shade" *ngIf="loading">
        <mat-spinner diameter="40"></mat-spinner>
      </div>

      <!-- Table -->
      <table mat-table [dataSource]="dataSource" matSort (matSortChange)="onSortChange($event)">
        <!-- Dynamic columns -->
        <ng-container *ngFor="let column of columns" [matColumnDef]="column.key">
          <th mat-header-cell *matHeaderCellDef 
              [mat-sort-header]="column.sortable !== false ? column.key : ''"
              [disabled]="column.sortable === false">
            {{ column.header }}
          </th>
          <td mat-cell *matCellDef="let row">
            <ng-container [ngSwitch]="column.type">
              <span *ngSwitchCase="'date'">{{ row[column.key] | date:'dd/MM/yyyy' }}</span>
              <span *ngSwitchCase="'currency'">{{ row[column.key] | currencyTn }}</span>
              <span *ngSwitchCase="'number'">{{ row[column.key] | number:'1.0-3' }}</span>
              <span *ngSwitchCase="'boolean'">
                <mat-icon [color]="row[column.key] ? 'primary' : 'warn'">
                  {{ row[column.key] ? 'check_circle' : 'cancel' }}
                </mat-icon>
              </span>
              <span *ngSwitchDefault>{{ row[column.key] }}</span>
            </ng-container>
          </td>
        </ng-container>

        <!-- Actions column -->
        <ng-container matColumnDef="actions" *ngIf="showActions">
          <th mat-header-cell *matHeaderCellDef>Actions</th>
          <td mat-cell *matCellDef="let row">
            <ng-content select="[row-actions]"></ng-content>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;" 
            (click)="onRowClick(row)"
            class="table-row"
            [class.clickable]="rowClick.observed">
        </tr>

        <!-- No data row -->
        <tr class="mat-row no-data-row" *matNoDataRow>
          <td class="mat-cell" [attr.colspan]="displayedColumns.length">
            <div class="no-data-message">
              <mat-icon>inbox</mat-icon>
              <span>Aucune donnée disponible</span>
            </div>
          </td>
        </tr>
      </table>

      <!-- Paginator -->
      <mat-paginator 
        [length]="totalItems"
        [pageSize]="pageSize"
        [pageSizeOptions]="pageSizeOptions"
        [showFirstLastButtons]="true"
        (page)="onPageChange($event)">
      </mat-paginator>
    </div>
  `,
  styles: [`
    .table-container {
      position: relative;
      overflow: auto;
    }

    .loading-shade {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(255, 255, 255, 0.8);
      z-index: 1;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    table {
      width: 100%;
    }

    .table-row {
      transition: background-color 0.2s;
    }

    .table-row:hover {
      background-color: #f5f5f5;
    }

    .table-row.clickable {
      cursor: pointer;
    }

    .no-data-row td {
      padding: 48px 0;
    }

    .no-data-message {
      display: flex;
      flex-direction: column;
      align-items: center;
      color: #999;
    }

    .no-data-message mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 8px;
    }

    mat-paginator {
      border-top: 1px solid #e0e0e0;
    }
  `]
})
export class DataTableComponent implements AfterViewInit, OnChanges {
  @Input() columns: TableColumn[] = [];
  @Input() data: any[] = [];
  @Input() loading: boolean = false;
  @Input() pageSizeOptions: number[] = [10, 25, 50, 100];
  @Input() pageSize: number = 10;
  @Input() totalItems: number = 0;
  @Input() showActions: boolean = false;

  @Output() rowClick = new EventEmitter<any>();
  @Output() pageChange = new EventEmitter<PageEvent>();
  @Output() sortChange = new EventEmitter<Sort>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  dataSource = new MatTableDataSource<any>();
  displayedColumns: string[] = [];

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data']) {
      this.dataSource.data = this.data;
    }
    if (changes['columns']) {
      this.displayedColumns = this.columns.map(c => c.key);
      if (this.showActions) {
        this.displayedColumns.push('actions');
      }
    }
    if (changes['showActions'] && this.columns.length > 0) {
      this.displayedColumns = this.columns.map(c => c.key);
      if (this.showActions) {
        this.displayedColumns.push('actions');
      }
    }
  }

  onRowClick(row: any): void {
    this.rowClick.emit(row);
  }

  onPageChange(event: PageEvent): void {
    this.pageChange.emit(event);
  }

  onSortChange(sort: Sort): void {
    this.sortChange.emit(sort);
  }
}
