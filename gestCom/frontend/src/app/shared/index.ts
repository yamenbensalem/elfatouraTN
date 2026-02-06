// Shared Module
export * from './shared.module';

// Components
export * from './components/loading-spinner/loading-spinner.component';
export * from './components/confirm-dialog/confirm-dialog.component';
export * from './components/page-header/page-header.component';
export * from './components/data-table/data-table.component';
export * from './components/search-input/search-input.component';

// Directives
export * from './directives/has-role.directive';

// Pipes
export * from './pipes/currency-tn.pipe';
export * from './pipes/date-fr.pipe';

// Interfaces
export type { TableColumn } from './components/data-table/data-table.component';
export type { ConfirmDialogData } from './components/confirm-dialog/confirm-dialog.component';
