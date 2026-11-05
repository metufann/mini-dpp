export interface ProductDto {
  gtin: string;
  modelName: string;
  batchNumber: string;
  productionPlace: string;
  cottonPercentage: number;
  elastanePercentage: number;
  carbonKgCO2e: number;
  version: number;
  updatedAt: string;
}

export interface ProductVersionDto {
  version: number;
  isCurrent: boolean;
  modelName: string;
  batchNumber: string;
  productionPlace: string;
  cottonPercentage: number;
  elastanePercentage: number;
  carbonKgCO2e: number;
  recordedAt: string;
}

export interface ImportRowErrorDto {
  row: number;
  reasons: string[];
}

export interface ImportResultDto {
  totalRows: number;
  savedRows: number;
  createdRows?: number;
  updatedRows?: number;
  unchangedRows?: number;
  failedRows: number;
  errors: ImportRowErrorDto[];
}
