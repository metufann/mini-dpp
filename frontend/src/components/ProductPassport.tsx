import { useState } from "react";
import { ExternalLink, Lock, ShieldCheck, Trash2, X } from "lucide-react";
import type { ProductDto } from "../types/product";
import { formatDecimal } from "../lib/format";
import { getHtmlDppUrl, getJsonLdUrl, getQrCodeUrl } from "../api/products";
import { useDeleteProduct } from "../hooks/useProducts";
import { VersionHistory } from "./VersionHistory";

interface ProductPassportProps {
  product: ProductDto;
  authorised: boolean;
  onClose?: () => void;
  onDelete?: () => void;
}

export function ProductPassport({
  product,
  authorised,
  onClose,
  onDelete,
}: ProductPassportProps) {
  const [isConfirmingDelete, setIsConfirmingDelete] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const { mutate: deleteProductMutate, isPending: isDeleting } =
    useDeleteProduct();

  const qrUrl = getQrCodeUrl(product.gtin);
  const jsonLdUrl = getJsonLdUrl(product.gtin, authorised);
  const htmlDppUrl = getHtmlDppUrl(product.gtin, authorised);

  function handleDelete() {
    setDeleteError(null);
    deleteProductMutate(product.gtin, {
      onSuccess: () => {
        onDelete?.();
      },
      onError: (err) => {
        setDeleteError(err.message);
      },
    });
  }

  return (
    <article
      aria-label={`Digital Product Passport for ${product.modelName}`}
      className="rounded-xl border border-neutral-200 bg-white p-5 sm:p-7"
    >
      {/* Header */}
      <div className="flex items-start justify-between border-b border-neutral-100 pb-5">
        <div>
          <div className="flex items-center gap-2">
            <span className="text-[11px] font-semibold uppercase tracking-wider text-neutral-400">
              Digital Product Passport
            </span>
            <span className="rounded bg-neutral-100 px-1.5 py-0.5 font-mono text-[10px] font-medium text-neutral-700">
              v{product.version ?? 1}
            </span>
            {authorised && (
              <span className="inline-flex items-center gap-1 rounded bg-neutral-900 px-1.5 py-0.5 text-[10px] font-medium text-white">
                <ShieldCheck size={11} aria-hidden="true" />
                Authorised (yetkili)
              </span>
            )}
          </div>
          <h2 className="mt-1 text-xl font-semibold tracking-tight text-neutral-900 sm:text-2xl">
            {product.modelName}
          </h2>
          <p className="mt-0.5 font-mono text-xs text-neutral-500">
            GTIN: {product.gtin}
          </p>
        </div>

        <div className="flex items-center gap-2">
          {onDelete && (
            isConfirmingDelete ? (
              <div className="inline-flex items-center gap-1.5">
                <span className="text-[11px] font-medium text-red-600">
                  Delete?
                </span>
                <button
                  type="button"
                  disabled={isDeleting}
                  onClick={handleDelete}
                  className="
                    rounded bg-red-600 px-2 py-0.5 text-[11px] font-medium text-white
                    hover:bg-red-700 disabled:opacity-50
                  "
                  aria-label="Confirm deletion"
                >
                  {isDeleting ? "…" : "Yes"}
                </button>
                <button
                  type="button"
                  disabled={isDeleting}
                  onClick={() => setIsConfirmingDelete(false)}
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
              <button
                type="button"
                onClick={() => setIsConfirmingDelete(true)}
                className="
                  inline-flex items-center gap-1 rounded-md px-2 py-1 text-xs text-neutral-400
                  transition-colors hover:bg-red-50 hover:text-red-600
                "
                title="Delete this product"
                aria-label={`Delete product ${product.modelName}`}
              >
                <Trash2 size={14} aria-hidden="true" />
                <span className="hidden sm:inline">Delete</span>
              </button>
            )
          )}

          {onClose && (
            <button
              type="button"
              onClick={onClose}
              className="rounded-md p-1.5 text-neutral-400 hover:bg-neutral-100 hover:text-neutral-700"
              aria-label="Close passport view"
            >
              <X size={18} />
            </button>
          )}
        </div>
      </div>

      {deleteError && (
        <div
          role="alert"
          className="mt-3 rounded-md border border-red-200 bg-red-50 p-2.5 text-xs text-red-700"
        >
          {deleteError}
        </div>
      )}

      <div className="mt-6 grid gap-8 lg:grid-cols-[1fr_220px]">
        {/* Main data specs */}
        <div className="space-y-6">
          {/* Section: Identification */}
          <div>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-neutral-400">
              Product Identification
            </h3>
            <dl className="mt-3 divide-y divide-neutral-100 text-xs">
              <div className="flex flex-col py-2 sm:flex-row sm:justify-between">
                <dt className="text-neutral-500">GTIN (Global Trade Item Number)</dt>
                <dd className="font-mono font-medium text-neutral-900">{product.gtin}</dd>
              </div>
              <div className="flex flex-col py-2 sm:flex-row sm:justify-between">
                <dt className="text-neutral-500">Model Name</dt>
                <dd className="font-medium text-neutral-900">{product.modelName}</dd>
              </div>
              <div className="flex flex-col py-2 sm:flex-row sm:justify-between">
                <dt className="text-neutral-500">Batch Number</dt>
                <dd className="font-mono font-medium text-neutral-900">{product.batchNumber}</dd>
              </div>
              <div className="flex flex-col py-2 sm:flex-row sm:justify-between">
                <dt className="text-neutral-500">Production Place</dt>
                <dd className="font-medium text-neutral-900">{product.productionPlace}</dd>
              </div>
            </dl>
          </div>

          {/* Section: Material Composition */}
          <div>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-neutral-400">
              Material Composition
            </h3>
            <div className="mt-3">
              <dl className="divide-y divide-neutral-100 text-xs">
                <div className="flex justify-between py-2">
                  <dt className="text-neutral-500">Cotton</dt>
                  <dd className="font-mono font-medium text-neutral-900">
                    {formatDecimal(product.cottonPercentage, "%", 0)}
                  </dd>
                </div>
                <div className="flex justify-between py-2">
                  <dt className="text-neutral-500">Elastane</dt>
                  <dd className="font-mono font-medium text-neutral-900">
                    {formatDecimal(product.elastanePercentage, "%", 0)}
                  </dd>
                </div>
              </dl>

              {/* Composition visual bar */}
              <div
                className="mt-2 flex h-2 w-full overflow-hidden rounded-full bg-neutral-100"
                aria-label={`Fiber ratio: ${product.cottonPercentage}% cotton, ${product.elastanePercentage}% elastane`}
              >
                <div
                  style={{ width: `${product.cottonPercentage}%` }}
                  className="bg-neutral-800"
                  title={`Cotton: ${product.cottonPercentage}%`}
                />
                <div
                  style={{ width: `${product.elastanePercentage}%` }}
                  className="bg-neutral-400"
                  title={`Elastane: ${product.elastanePercentage}%`}
                />
              </div>
              <div className="mt-1.5 flex justify-between text-[10px] text-neutral-400">
                <span>● Cotton ({product.cottonPercentage}%)</span>
                <span>● Elastane ({product.elastanePercentage}%)</span>
              </div>
            </div>
          </div>

          {/* Section: Environmental & ESG */}
          <div>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-neutral-400">
              Environmental &amp; Sustainability (ESG)
            </h3>
            <div className="mt-3">
              {authorised ? (
                <dl className="divide-y divide-neutral-100 text-xs">
                  <div className="flex flex-col py-2 sm:flex-row sm:justify-between">
                    <dt className="text-neutral-500">Carbon Footprint</dt>
                    <dd className="font-mono font-semibold text-neutral-900">
                      {formatDecimal(product.carbonKgCO2e, "kg CO₂e", 4)}
                    </dd>
                  </div>
                  <div className="flex flex-col py-2 sm:flex-row sm:justify-between">
                    <dt className="text-neutral-500">Access Role</dt>
                    <dd className="text-neutral-700">Authorised (rol=yetkili)</dd>
                  </div>
                </dl>
              ) : (
                <div className="flex items-start gap-2.5 rounded-lg border border-neutral-200 bg-neutral-50 p-3 text-xs text-neutral-600">
                  <Lock size={15} className="mt-0.5 shrink-0 text-neutral-400" />
                  <div>
                    <p className="font-medium text-neutral-800">Restricted Environmental Data</p>
                    <p className="mt-0.5 text-neutral-500">
                      Carbon footprint information is restricted under ESG visibility rules.
                      Switch on &ldquo;Authorised View&rdquo; in the header to view this data.
                    </p>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Aside: QR Code & External API Links */}
        <aside className="flex flex-col items-center border-t border-neutral-100 pt-6 lg:border-l lg:border-t-0 lg:pl-6 lg:pt-0">
          <div className="rounded-lg border border-neutral-200 bg-white p-3 shadow-xs">
            <img
              src={qrUrl}
              alt={`QR code for ${product.gtin}`}
              width={160}
              height={160}
              className="h-40 w-40"
              loading="lazy"
            />
          </div>
          <p className="mt-2 text-center text-[11px] text-neutral-400">
            Scan to open public DPP
          </p>

          <div className="mt-5 flex w-full flex-col gap-2 border-t border-neutral-100 pt-4 text-xs">
            <a
              href={htmlDppUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="
                inline-flex items-center justify-between rounded-md border border-neutral-200
                bg-white px-3 py-1.5 text-neutral-700 transition-colors hover:bg-neutral-50
                focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-neutral-900
              "
            >
              <span>Public HTML Passport</span>
              <ExternalLink size={12} className="text-neutral-400" />
            </a>

            <a
              href={jsonLdUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="
                inline-flex items-center justify-between rounded-md border border-neutral-200
                bg-white px-3 py-1.5 text-neutral-700 transition-colors hover:bg-neutral-50
                focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-neutral-900
              "
            >
              <span>JSON-LD Document</span>
              <ExternalLink size={12} className="text-neutral-400" />
            </a>
          </div>
        </aside>
      </div>

      {/* Version History Section */}
      <div className="mt-8 border-t border-neutral-100 pt-6">
        <VersionHistory
          gtin={product.gtin}
          currentVersion={product.version ?? 1}
          authorised={authorised}
        />
      </div>
    </article>
  );
}
