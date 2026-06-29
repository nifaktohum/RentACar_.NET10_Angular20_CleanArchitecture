import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'codeFormatPipe'
})
export class CodeFormatPipe implements PipeTransform {

  transform(value: string | null | undefined): string {
    if (!value) return '';

    // // 1. İçindeki tüm tireleri veya sayı dışı karakterleri temizle kanka
    let cleaned = value.replace(/\D/g, '');

    // // 2. Eğer kullanıcı ilk 3 haneyi girdiyse (Örn: 123), direkt düz göster
    if (cleaned.length <= 3) {
      return cleaned;
    }

    // // 3. Eğer 3 haneden fazla girildiyse, tam ortasına jilet gibi tireyi yapıştır kanka (Örn: 123-4)
    const part1 = cleaned.substring(0, 3);
    const part2 = cleaned.substring(3, 6); // Maksimum 6 karakter olacak şekilde sınırla

    return `${part1}-${part2}`;
  }

}
