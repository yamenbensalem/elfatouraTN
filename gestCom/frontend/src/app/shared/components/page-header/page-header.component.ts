import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  standalone: false,
  template: `
    <div class="page-header">
      <div class="header-content">
        <div class="title-section">
          <h1 class="page-title">{{ title }}</h1>
          <p class="page-subtitle" *ngIf="subtitle">{{ subtitle }}</p>
        </div>
        <div class="actions-section">
          <ng-content select="[actions]"></ng-content>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-header {
      margin-bottom: 24px;
      padding-bottom: 16px;
      border-bottom: 1px solid #e0e0e0;
    }

    .header-content {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      flex-wrap: wrap;
      gap: 16px;
    }

    .title-section {
      flex: 1;
      min-width: 200px;
    }

    .page-title {
      margin: 0;
      font-size: 24px;
      font-weight: 500;
      color: #333;
    }

    .page-subtitle {
      margin: 8px 0 0 0;
      font-size: 14px;
      color: #666;
    }

    .actions-section {
      display: flex;
      gap: 8px;
      align-items: center;
      flex-wrap: wrap;
    }

    @media (max-width: 600px) {
      .header-content {
        flex-direction: column;
      }

      .actions-section {
        width: 100%;
        justify-content: flex-start;
      }
    }
  `]
})
export class PageHeaderComponent {
  @Input() title: string = '';
  @Input() subtitle?: string;
}
