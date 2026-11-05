import { useState, useMemo } from "react";
import {
  AlertCircle,
  ArrowUpRight,
  Lock,
  Search,
  Trash2,
} from "lucide-react";
import type { ProductDto } from "../types/product";
import { formatDecimal } from "../lib/format";
import { useDeleteProduct } from "../hooks/useProducts";

interface ProductListSectionProps {
  products: ProductDto[] | undefined;
  isLoading: boolean;
  authorised: boolean;
  selectedGtin: string | null;
  onSelectProduct: (gtin: string) => void;
  onProductDeleted?: (gtin: string) => void;
}

export function ProductListSection({
  products,
  isLoading,
  authorised,
  selectedGtin,
  onSelectProduct,
  onProductDeleted,
}: ProductListSectionProps) {
  const [filterQuery, setFilterQuery] = useState("");
  const [confirmingGtin, setConfirmingGtin] = useState<string | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const { mutate: deleteProductMutate, isPending: isDeleting } =
    useDeleteProduct();

  const filteredProducts = useMemo(() => {
    if (!products) return [];
    if (!filterQuery.trim()) return products;
    const q = filterQuery.toLowerCase().trim();
    return products.filter(
      (p) =>
        p.gtin.toLowerCase().includes(q) ||
        p.modelName.toLowerCase().includes(q) ||
        p.batchNumber.toLowerCase().includes(q) ||
        p.productionPlace.toLowerCase().includes(q),
    );
  }, [products, filterQuery]);

  function handleDelete(gtin: string) {
    setDeleteError(null);
    deleteProductMutate(gtin, {
      onSuccess: () => {
        setConfirmingGtin(null);
        onProductDeleted?.(gtin);
      },
      onError: (err) => {
        setDeleteError(err.message);
      },
    });
  }

  return (
    <section
      aria-labelledby="catalog-title"
      className="rounded-xl border border-neutral-200 bg-white p-5 sm:p-6"
    >
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <div className="flex items-center gap-2">
            <h2
              id="catalog-title"
              className="text-sm font-semibold tracking-tight text-neutral-900"
            >
              Registered Products
            </h2>
            {products && (
              <span className="rounded-full bg-neutral-100 px-2 py-0.5 font-mono text-[11px] text-neutral-600">
                {products.length}
              </span>
            )}
          </div>
          <p className="mt-0.5 text-xs text-neutral-500">
            Select a product to view its passport or manage registered records.
          </p>
        </div>

        {/* Filter input */}
        {products && products.length > 0 && (
          <div className="relative w-full sm:w-64">
            <Search
              size={14}
              className="absolute left-2.5 top-1/2 -translate-y-1/2 text-neutral-400"
              aria-hidden="true"
            />
            <input
              type="text"
              value={filterQuery}
              onChange={(e) => setFilterQuery(e.target.value)}
              placeholder="Filter catalog…"
              className="
                w-full rounded-md border border-neutral-200 bg-white py-1.5 pl-8 pr-3
                text-xs text-neutral-900 placeholder:text-neutral-400
                focus:border-neutral-400 focus:outline-none focus:ring-1 focus:ring-neutral-400
              "
              aria-label="Filter products catalog"
            />
          </div>
        )}
      </div>

      {/* Delete error notification */}
      {deleteError && (
        <div
          role="alert"
          className="mt-3 flex items-center justify-between rounded-md border border-red-200 bg-red-50 p-2.5 text-xs text-red-700"
        >
          <div className="flex items-center gap-2">
            <AlertCircle size={14} className="shrink-0 text-red-600" />
            <span>{deleteError}</span>
          </div>
          <button
            type="button"
            onClick={() => setDeleteError(null)}
            className="text-xs font-medium text-red-600 hover:text-red-800"
          >
            Dismiss
          </button>
        </div>
      )}

      {/* Loading state */}
      {isLoading && (
        <div className="py-12 text-center" role="status">
          <div
            className="mx-auto h-5 w-5 animate-spin rounded-full border-2 border-neutral-200 border-t-neutral-800"
            style={{ animationDuration: "0.7s" }}
          />
          <p className="mt-3 text-xs text-neutral-400">Loading catalog…</p>
        </div>
      )}

      {/* Empty catalog state */}
      {!isLoading && products && products.length === 0 && (
        <div className="py-12 text-center">
          <p className="text-xs text-neutral-500">
            No products in database yet.
          </p>
          <p className="mt-1 text-xs text-neutral-400">
            Upload a CSV file above to register products into the passport repository.
          </p>
        </div>
      )}

      {/* Filter returned no results */}
      {!isLoading && products && products.length > 0 && filteredProducts.length === 0 && (
        <div className="py-8 text-center text-xs text-neutral-400">
          No products match &ldquo;{filterQuery}&rdquo;.
        </div>
      )}

      {/* Products table */}
      {!isLoading && filteredProducts.length > 0 && (
        <div className="mt-4 overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead>
              <tr className="border-b border-neutral-200 text-neutral-400">
                <th className="pb-2 font-medium">GTIN</th>
                <th className="pb-2 font-medium">Ver</th>
                <th className="pb-2 font-medium">Model</th>
                <th className="pb-2 font-medium">Batch</th>
                <th className="pb-2 font-medium">Origin</th>
                <th className="pb-2 font-medium">Composition</th>
                <th className="pb-2 font-medium">Carbon</th>
                <th className="pb-2 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-neutral-100">
              {filteredProducts.map((p) => {
                const isSelected = selectedGtin === p.gtin;
                const isConfirming = confirmingGtin === p.gtin;

                return (
                  <tr
                    key={p.gtin}
                    onClick={() => onSelectProduct(p.gtin)}
                    className={`
                      cursor-pointer transition-colors
                      ${
                        isSelected
                          ? "bg-neutral-100/80 font-medium"
                          : "hover:bg-neutral-50"
                      }
                    `}
                  >
                    <td className="py-2.5 pr-3 font-mono font-medium text-neutral-900">
                      {p.gtin}
                    </td>
                    <td className="py-2.5 pr-3 font-mono text-[11px] text-neutral-500">
                      <span className="rounded bg-neutral-100 px-1.5 py-0.5 font-medium">
                        v{p.version ?? 1}
                      </span>
                    </td>
                    <td className="py-2.5 pr-3 text-neutral-800">
                      {p.modelName}
                    </td>
                    <td className="py-2.5 pr-3 font-mono text-neutral-600">
                      {p.batchNumber}
                    </td>
                    <td className="py-2.5 pr-3 text-neutral-600">
                      {p.productionPlace}
                    </td>
                    <td className="py-2.5 pr-3 text-neutral-600">
                      {formatDecimal(p.cottonPercentage, "%", 0)} cotton /{" "}
                      {formatDecimal(p.elastanePercentage, "%", 0)} elastane
                    </td>
                    <td className="py-2.5 pr-3">
                      {authorised ? (
                        <span className="font-mono text-neutral-900">
                          {formatDecimal(p.carbonKgCO2e, "kg CO₂e", 2)}
                        </span>
                      ) : (
                        <span
                          className="inline-flex items-center gap-1 text-[11px] text-neutral-400"
                          title="Restricted (rol=yetkili)"
                        >
                          <Lock size={11} aria-hidden="true" />
                          Restricted
                        </span>
                      )}
                    </td>
                    <td className="py-2.5 text-right whitespace-nowrap">
                      {isConfirming ? (
                        <div
                          className="inline-flex items-center gap-1.5"
                          onClick={(e) => e.stopPropagation()}
                        >
                          <span className="text-[11px] font-medium text-red-600">
                            Delete?
                          </span>
                          <button
                            type="button"
                            disabled={isDeleting}
                            onClick={() => handleDelete(p.gtin)}
                            className="
                              rounded bg-red-600 px-2 py-0.5 text-[11px] font-medium text-white
                              hover:bg-red-700 disabled:opacity-50
                            "
                            aria-label={`Confirm delete product ${p.gtin}`}
                          >
                            {isDeleting ? "…" : "Yes"}
                          </button>
                          <button
                            type="button"
                            disabled={isDeleting}
                            onClick={() => setConfirmingGtin(null)}
                            className="
                              rounded border border-neutral-200 px-1.5 py-0.5 text-[11px] text-neutral-600
                              hover:bg-neutral-100 disabled:opacity-50
                            "
                            aria-label="Cancel deletion"
                          >
                            Cancel
                          </button>
                        </div>
                      ) : (
                        <div className="inline-flex items-center gap-1">
                          <button
                            type="button"
                            onClick={(e) => {
                              e.stopPropagation();
                              onSelectProduct(p.gtin);
                            }}
                            className="
                              inline-flex items-center gap-1 rounded px-2 py-1 text-xs text-neutral-600
                              hover:bg-neutral-200/70 hover:text-neutral-900
                              focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-neutral-900
                            "
                            aria-label={`Open passport for ${p.modelName} ${p.gtin}`}
                          >
                            <span>View</span>
                            <ArrowUpRight size={13} aria-hidden="true" />
                          </button>

                          <button
                            type="button"
                            onClick={(e) => {
                              e.stopPropagation();
                              setConfirmingGtin(p.gtin);
                            }}
                            className="
                              inline-flex items-center rounded p-1 text-neutral-400
                              hover:bg-red-50 hover:text-red-600
                              focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-red-600
                            "
                            title="Delete product"
                            aria-label={`Delete product ${p.modelName} ${p.gtin}`}
                          >
                            <Trash2 size={13} aria-hidden="true" />
                          </button>
                        </div>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
