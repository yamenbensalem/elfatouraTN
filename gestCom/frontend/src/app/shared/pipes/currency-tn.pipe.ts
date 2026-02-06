import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'currencyTn',
  standalone: false
})
export class CurrencyTnPipe implements PipeTransform {
  /**
   * Transforms a number to Tunisian Dinar format
   * Example: 1234.567 -> "1 234,567 TND"
   */
  transform(value: number | string | null | undefined, showSymbol: boolean = true): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }

    const numValue = typeof value === 'string' ? parseFloat(value) : value;

    if (isNaN(numValue)) {
      return '';
    }

    // Format with 3 decimal places
    const formatted = numValue.toFixed(3);
    
    // Split into integer and decimal parts
    const [integerPart, decimalPart] = formatted.split('.');
    
    // Add thousand separators (space) to integer part
    const formattedInteger = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
    
    // Combine with comma as decimal separator (French format)
    const result = `${formattedInteger},${decimalPart}`;
    
    return showSymbol ? `${result} TND` : result;
  }
}
