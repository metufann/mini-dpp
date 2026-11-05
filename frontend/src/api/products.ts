import { API_BASE_URL } from "../lib/constants";
import type { ImportResultDto, ProductDto, ProductVersionDto } from "../types/product";

export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

/**
 * Fetch all products in the catalog.
 */
export async function fetchProducts(): Promise<ProductDto[]> {
  const response = await fetch(`${API_BASE_URL}/api/products`);
  if (!response.ok) {
    throw new ApiError(
      response.status,
      `Failed to load products (${response.status}).`,
    );
  }
  return response.json() as Promise<ProductDto[]>;
}

/**
 * Fetch a single product by GTIN.
 */
export async function fetchProduct(gtin: string): Promise<ProductDto> {
  const response = await fetch(
    `${API_BASE_URL}/api/products/${encodeURIComponent(gtin)}`,
  );
  if (response.status === 404) {
    throw new ApiError(404, "Product not found.");
  }
  if (!response.ok) {
    throw new ApiError(
      response.status,
      `Server error (${response.status}).`,
    );
  }
  return response.json() as Promise<ProductDto>;
}

/**
 * Fetch version history for a product.
 */
export async function fetchProductVersions(
  gtin: string,
): Promise<ProductVersionDto[]> {
  const response = await fetch(
    `${API_BASE_URL}/api/products/${encodeURIComponent(gtin)}/versions`,
  );
  if (response.status === 404) {
    throw new ApiError(404, "Product not found.");
  }
  if (!response.ok) {
    throw new ApiError(
      response.status,
      `Failed to load version history (${response.status}).`,
    );
  }
  return response.json() as Promise<ProductVersionDto[]>;
}

/**
 * Delete a product by GTIN.
 */
export async function deleteProduct(gtin: string): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/api/products/${encodeURIComponent(gtin)}`,
    {
      method: "DELETE",
    },
  );
  if (response.status === 404) {
    throw new ApiError(404, "Product not found.");
  }
  if (!response.ok) {
    throw new ApiError(
      response.status,
      `Failed to delete product (${response.status}).`,
    );
  }
}

/**
 * Upload a CSV file to POST /api/import.
 */
export async function uploadCsv(file: File): Promise<ImportResultDto> {
  const formData = new FormData();
  formData.append("file", file, file.name);

  const response = await fetch(`${API_BASE_URL}/api/import`, {
    method: "POST",
    body: formData,
  });

  const contentType = response.headers.get("content-type") ?? "";
  const isJson = contentType.includes("application/json");

  if (!response.ok) {
    let errorMessage = `Upload failed (${response.status}).`;
    if (isJson) {
      const errorData = (await response.json()) as { message?: string };
      if (errorData.message) {
        errorMessage = errorData.message;
      }
    } else {
      const text = await response.text();
      if (text) errorMessage = text;
    }
    throw new ApiError(response.status, errorMessage);
  }

  return response.json() as Promise<ImportResultDto>;
}

/**
 * Build the URL for the backend QR code image.
 */
export function getQrCodeUrl(gtin: string): string {
  return `${API_BASE_URL}/api/products/${encodeURIComponent(gtin)}/qr`;
}

/**
 * Build the URL for the backend JSON-LD document.
 */
export function getJsonLdUrl(gtin: string, authorised = false): string {
  const base = `${API_BASE_URL}/dpp/${encodeURIComponent(gtin)}.jsonld`;
  return authorised ? `${base}?rol=yetkili` : base;
}

/**
 * Build the URL for the backend DPP HTML page.
 */
export function getHtmlDppUrl(gtin: string, authorised = false): string {
  const base = `${API_BASE_URL}/dpp/${encodeURIComponent(gtin)}`;
  return authorised ? `${base}?rol=yetkili` : base;
}
