export const DRIVER_LICENSE_CLASSES: DriverLicenseClassOption[] = [
  { label: 'A (Motosiklet)', value: 'A' },
  { label: 'A1 (Hafif Motosiklet)', value: 'A1' },
  { label: 'A2 (Orta Güçte Motosiklet)', value: 'A2' },
  { label: 'B (Otomobil / Kamyonet)', value: 'B' },
  { label: 'B1 (Dört Tekerlekli Motosiklet)', value: 'B1' },
  { label: 'BE (Römorklu Otomobil)', value: 'BE' },
  { label: 'C (Kamyon / Çekici)', value: 'C' },
  { label: 'C1 (Hafif Kamyon)', value: 'C1' },
  { label: 'D (Otobüs)', value: 'D' },
  { label: 'D1 (Minibüs)', value: 'D1' },
  { label: 'F (Traktör)', value: 'F' },
  { label: 'G (İş Makinesi)', value: 'G' }
];


export interface DriverLicenseClassOption {
  label: string;
  value: string;
}