import { Pipe, PipeTransform } from '@angular/core';



@Pipe({
  name: 'phoneFormatPipe',
  standalone: true
})
export class PhoneFormatPipe implements PipeTransform {

  transform(value: string | number | null | undefined): string {
    if (value === null || value === undefined || value.toString().trim() === '') {
      return '----------';
    }

    // // 1. Gelen veriyi string'e çevir ve sadece sayıları ayıkla 
    let cleaned = ('' + value).replace(/\D/g, '');

    // // Kullanıcı yazmaya başladıysa ve numaranın başında Türkiye kodu (90) veya '0' yoksa,
    // // (Örn: direkt '5' diye yazmaya başladıysa) başına otomatik 90 ekliyoruz .
    if (cleaned.length > 0 && !cleaned.startsWith('90') && !cleaned.startsWith('0') && !cleaned.startsWith('1')) {
      cleaned = '90' + cleaned;
    }
    // // 2. Eğer numaranın başında '0' varsa (Örn: 0553...), uluslararası formata uyması için onu '90' yapıyoruz
    if (cleaned.startsWith('0') && cleaned.length === 11) {
      cleaned = '90' + cleaned.substring(1);
    }

    // // 3. SENARYO A: Klasik Türkiye Numarası formatı (Örn: 905538740708 - 12 Hane)
    if (cleaned.length === 12 && cleaned.startsWith('90')) {
      const countryCode = cleaned.substring(0, 2); // 90
      const areaCode = cleaned.substring(2, 5);    // 553
      const part1 = cleaned.substring(5, 8);       // 874
      const part2 = cleaned.substring(8, 10);      // 07
      const part3 = cleaned.substring(10, 12);     // 08

      return `+${countryCode} (${areaCode}) ${part1} ${part2} ${part3}`;
    } else if (cleaned.length === 0) {
      return '+----------'; // Telefon formatı için boş şablon
    }
    
    

    // // 4. SENARYO B: Eksik veya hatalı girişlerde ara format koruması (Örn: Sadece 905 yazdıysa)
    if (cleaned.startsWith('90') && cleaned.length < 12) {
      const countryCode = cleaned.substring(0, 2);
      const rest = cleaned.substring(2);
      return `+${countryCode} ${rest}`;
    }

    // // 6. Fallback: Eğer yukarıdakilere uymuyorsa ham halini dön 
    return value.toString().startsWith('+') ? value.toString() : `+${value}`;

    
  }
}