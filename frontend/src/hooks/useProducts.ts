import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  deleteProduct,
  fetchProduct,
  fetchProducts,
  fetchProductVersions,
  uploadCsv,
} from "../api/products";

export function useProducts() {
  return useQuery({
    queryKey: ["products"],
    queryFn: fetchProducts,
    staleTime: 60 * 1000,
  });
}

export function useProduct(gtin: string | null, enabled = true) {
  return useQuery({
    queryKey: ["product", gtin],
    queryFn: () =>
      gtin
        ? fetchProduct(gtin)
        : Promise.reject(new Error("No GTIN provided")),
    enabled: Boolean(enabled && gtin && gtin.trim().length > 0),
    retry: false,
    staleTime: 5 * 60 * 1000,
  });
}

export function useProductVersions(gtin: string | null, enabled = true) {
  return useQuery({
    queryKey: ["product-versions", gtin],
    queryFn: () =>
      gtin
        ? fetchProductVersions(gtin)
        : Promise.reject(new Error("No GTIN provided")),
    enabled: Boolean(enabled && gtin && gtin.trim().length > 0),
    staleTime: 60 * 1000,
  });
}

export function useImportCsv() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (file: File) => uploadCsv(file),
    onSuccess: () => {
      // Refresh the product listing and details after an import
      void queryClient.invalidateQueries({ queryKey: ["products"] });
      void queryClient.invalidateQueries({ queryKey: ["product"] });
      void queryClient.invalidateQueries({ queryKey: ["product-versions"] });
    },
  });
}

export function useDeleteProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (gtin: string) => deleteProduct(gtin),
    onSuccess: () => {
      // Refresh the product listing after a deletion
      void queryClient.invalidateQueries({ queryKey: ["products"] });
      void queryClient.invalidateQueries({ queryKey: ["product"] });
      void queryClient.invalidateQueries({ queryKey: ["product-versions"] });
    },
  });
}
