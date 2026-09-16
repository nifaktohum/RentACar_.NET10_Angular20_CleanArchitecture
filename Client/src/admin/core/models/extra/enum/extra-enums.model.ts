
export const PriceTypeValues = {
  Daily: 1,
  Rental: 2
} as const;

// export const PriceTypeLabels: Record<number, string> = {
//   1: 'Günlük',
//   2: 'Kiralama Başı'
// };

export const PriceTypeLabels: Record<string, string> = {
  'Daily': 'Günlük',
  'Rental': 'Kiralama Başı'
};

// export const PriceTypeOptions = [
//   { label: 'Günlük', value: 1 },
//   { label: 'Kiralama Başı', value: 2 }
// ];
// export const ExtraPriceTypeOptions = {
//   1: 'Daily',
//   2: 'Rental',
// } satisfies Record<number, string>;


export const ExtraCategoryValues = {
  Guarantee: 1,
  Driver: 2,
  Seat: 3,
  Other: 4
} as const;

// export const ExtraCategoryLabels: Record<number, string> = {
//   1: 'Güvence',
//   2: 'Sürücü',
//   3: 'Koltuk',
//   4: 'Diğer'
// };

export const ExtraCategoryLabels: Record<string, string> = {
  'Guarantee': 'Güvence',
  'Driver': 'Sürücü',
  'Seat': 'Koltuk',
  'Other': 'Diğer'
};

export const ExtraCategoriesConfig: Record<string, { label: string, value: number }> = {
  'Guarantee': { label: 'Güvence', value: 1 },
  'Driver': { label: 'Sürücü', value: 2 },
  'Seat': { label: 'Koltuk', value: 3 },
  'Other': { label: 'Diğer', value: 4 }
};




export const ExtraPriceTypeOptions = [
  { label: 'Günlük', value: 1 },
  { label: 'Kiralama Başı', value: 2 },
];

export const ExtraCategoryOptions = [
  { label: 'Güvence', value: 1 },
  { label: 'Sürücü', value: 2 },
  { label: 'Koltuk', value: 3 },
  { label: 'Diğer', value: 4 }
];

export const CategoryMap: Record<number, string> = {
  1: 'Guarantee',
  2: 'Driver',
  3: 'Seat',
  4: 'Other'
};

export const PriceTypeMap: Record<number, string> = {
  1: 'Daily',
  2: 'Rental'
};