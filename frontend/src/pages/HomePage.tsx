import { useState, useCallback } from "react";
import { Header } from "../components/Header";
import { CsvUploadSection } from "../components/CsvUploadSection";
import { SearchBar } from "../components/SearchBar";
import { ProductListSection } from "../components/ProductListSection";
import { ProductPassport } from "../components/ProductPassport";
import { LoadingState } from "../components/LoadingState";
import { ErrorState } from "../components/ErrorState";
import { useProduct, useProducts } from "../hooks/useProducts";
import { ApiError } from "../api/products";

export function HomePage() {
  const [authorised, setAuthorised] = useState(false);
  const [selectedGtin, setSelectedGtin] = useState<string | null>(null);

  // Load all products in the repository
  const {
    data: products,
    isLoading: isLoadingProducts,
    refetch: refetchProducts,
  } = useProducts();

  // Load selected product by GTIN
  const {
    data: product,
    isLoading: isLoadingProduct,
    isError: isProductError,
    error: productError,
  } = useProduct(selectedGtin, Boolean(selectedGtin));

  const handleSelectProduct = useCallback((gtin: string) => {
    setSelectedGtin(gtin);
    // Smooth scroll to passport view if on mobile/small screen
    window.scrollTo({ top: 350, behavior: "smooth" });
  }, []);

  const handleClosePassport = useCallback(() => {
    setSelectedGtin(null);
  }, []);

  const handleProductDeleted = useCallback(
    (gtin: string) => {
      if (selectedGtin === gtin) {
        setSelectedGtin(null);
      }
    },
    [selectedGtin],
  );

  function getErrorMessage(): string {
    if (productError instanceof ApiError) {
      if (productError.status === 404) {
        return `No product found for GTIN "${selectedGtin}".`;
      }
      return productError.message;
    }
    if (productError instanceof TypeError && productError.message.includes("fetch")) {
      return "Cannot connect to the backend API. Please ensure the server is running on http://localhost:5141.";
    }
    return "An unexpected error occurred while fetching product data.";
  }

  return (
    <div className="flex min-h-screen flex-col bg-neutral-50/40 text-neutral-900">
      <Header authorised={authorised} onAuthorisedChange={setAuthorised} />

      <main className="flex-1">
        <div className="mx-auto max-w-5xl space-y-6 px-4 py-8 sm:px-6">
          {/* Section 1: CSV Upload */}
          <CsvUploadSection
            onImportSuccess={() => {
              void refetchProducts();
            }}
          />

          {/* Section 2: Direct GTIN Lookup */}
          <section
            aria-labelledby="lookup-title"
            className="rounded-xl border border-neutral-200 bg-white p-5 sm:p-6"
          >
            <div className="mb-3">
              <h2
                id="lookup-title"
                className="text-sm font-semibold tracking-tight text-neutral-900"
              >
                Look up by GTIN
              </h2>
              <p className="mt-0.5 text-xs text-neutral-500">
                Directly retrieve a passport using its 14-digit Global Trade Item Number.
              </p>
            </div>
            <SearchBar
              onSearch={handleSelectProduct}
              isLoading={isLoadingProduct}
            />
          </section>

          {/* Section 3: Passport Display (when GTIN is selected) */}
          {selectedGtin && (
            <div id="passport-view" className="scroll-mt-6">
              {isLoadingProduct && <LoadingState />}

              {isProductError && (
                <div className="rounded-xl border border-neutral-200 bg-white p-6">
                  <ErrorState
                    title="Product Not Found"
                    message={getErrorMessage()}
                    onRetry={handleClosePassport}
                  />
                </div>
              )}

              {product && !isLoadingProduct && !isProductError && (
                <ProductPassport
                  product={product}
                  authorised={authorised}
                  onClose={handleClosePassport}
                  onDelete={() => setSelectedGtin(null)}
                />
              )}
            </div>
          )}

          {/* Section 4: Registered Products Catalog */}
          <ProductListSection
            products={products}
            isLoading={isLoadingProducts}
            authorised={authorised}
            selectedGtin={selectedGtin}
            onSelectProduct={handleSelectProduct}
            onProductDeleted={handleProductDeleted}
          />
        </div>
      </main>

      <footer className="border-t border-neutral-200 bg-white py-6 text-center text-xs text-neutral-400">
        <div className="mx-auto max-w-5xl px-4 sm:px-6">
          Mini-DPP · Digital Product Passport Demonstration · Invariant Culture GS1 Standard
        </div>
      </footer>
    </div>
  );
}
