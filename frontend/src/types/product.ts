export interface ProductDto {
  gtin: string;
  modelName: string;
  batchNumber: string;
  productionPlace: string;
  cottonPercentage: number;
  elastanePercentage: number;
  carbonKgCO2e: number;
}

export interface ImportRowErrorDto {
  row: number;
  reasons: string[];
}

export interface ImportResultDto {
  totalRows: number;
  savedRows: number;
  failedRows: number;
  errors: ImportRowErrorDto[];
}
