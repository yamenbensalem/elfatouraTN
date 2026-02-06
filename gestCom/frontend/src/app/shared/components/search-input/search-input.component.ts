import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

@Component({
  selector: 'app-search-input',
  standalone: false,
  template: `
    <mat-form-field appearance="outline" class="search-field">
      <mat-icon matPrefix>search</mat-icon>
      <input matInput 
             [formControl]="searchControl" 
             [placeholder]="placeholder"
             type="text">
      <button mat-icon-button 
              matSuffix 
              *ngIf="searchControl.value" 
              (click)="clearSearch()"
              aria-label="Effacer la recherche">
        <mat-icon>close</mat-icon>
      </button>
    </mat-form-field>
  `,
  styles: [`
    .search-field {
      width: 100%;
      max-width: 400px;
    }

    :host ::ng-deep .search-field .mat-mdc-form-field-infix {
      padding-top: 8px;
      padding-bottom: 8px;
    }

    :host ::ng-deep .search-field .mat-mdc-text-field-wrapper {
      background-color: #fafafa;
    }

    mat-icon[matPrefix] {
      color: #999;
      margin-right: 8px;
    }
  `]
})
export class SearchInputComponent implements OnInit, OnDestroy {
  @Input() placeholder: string = 'Rechercher...';
  @Input() debounceTime: number = 300;
  @Output() search = new EventEmitter<string>();

  searchControl = new FormControl('');
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(this.debounceTime),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(value => {
        this.search.emit(value || '');
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  clearSearch(): void {
    this.searchControl.setValue('');
  }
}
