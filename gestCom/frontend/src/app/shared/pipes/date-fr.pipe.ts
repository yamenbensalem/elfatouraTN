import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'dateFr',
  standalone: false
})
export class DateFrPipe implements PipeTransform {
  /**
   * Transforms a Date to French format dd/MM/yyyy
   * Example: 2024-03-15 -> "15/03/2024"
   */
  transform(value: Date | string | null | undefined, format: 'short' | 'long' | 'datetime' = 'short'): string {
    if (!value) {
      return '';
    }

    const date = value instanceof Date ? value : new Date(value);

    if (isNaN(date.getTime())) {
      return '';
    }

    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();

    switch (format) {
      case 'long':
        return this.formatLong(date);
      case 'datetime':
        const hours = date.getHours().toString().padStart(2, '0');
        const minutes = date.getMinutes().toString().padStart(2, '0');
        return `${day}/${month}/${year} ${hours}:${minutes}`;
      case 'short':
      default:
        return `${day}/${month}/${year}`;
    }
  }

  private formatLong(date: Date): string {
    const months = [
      'janvier', 'février', 'mars', 'avril', 'mai', 'juin',
      'juillet', 'août', 'septembre', 'octobre', 'novembre', 'décembre'
    ];

    const day = date.getDate();
    const month = months[date.getMonth()];
    const year = date.getFullYear();

    return `${day} ${month} ${year}`;
  }
}
