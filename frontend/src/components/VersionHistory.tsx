import { useState } from "react";
import { History, Lock, ChevronDown, ChevronUp } from "lucide-react";
import { useProductVersions } from "../hooks/useProducts";
import { formatDateTime, formatDecimal } from "../lib/format";

interface VersionHistoryProps {
  gtin: string;
  currentVersion: number;
  authorised: boolean;
}

export function VersionHistory({
  gtin,
  currentVersion,
  authorised,
}: VersionHistoryProps) {
  const [isOpen, setIsOpen] = useState(false);
  const { data: versions, isLoading, isError } = useProductVersions(gtin);

  const totalVersions = versions?.length ?? (currentVersion || 1);

  return (
    <div className="rounded-xl border border-neutral-200 bg-white p-5 sm:p-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2.5">
          <div className="flex h-7 w-7 items-center justify-center rounded-md bg-neutral-100 text-neutral-600">
            <History size={15} aria-hidden="true" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h3 className="text-xs font-semibold uppercase tracking-wider text-neutral-800">
                Version History
              </h3>
              <span className="rounded-full bg-neutral-100 px-2 py-0.5 font-mono text-[11px] text-neutral-600">
                v{currentVersion} · {totalVersions} {totalVersions === 1 ? "entry" : "entries"}
              </span>
            </div>
            <p className="mt-0.5 text-xs text-neutral-500">
              Audit log of product updates and specification changes.
            </p>
          </div>
        </div>

        <button
          type="button"
          onClick={() => setIsOpen((prev) => !prev)}
          className="
            inline-flex items-center gap-1.5 rounded-md border border-neutral-200 px-2.5 py-1.5
            text-xs font-medium text-neutral-700 transition-colors hover:bg-neutral-50
            focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-neutral-900
          "
          aria-expanded={isOpen}
          aria-label={isOpen ? "Collapse version history" : "Expand version history"}
        >
          <span>{isOpen ? "Hide history" : "View history"}</span>
          {isOpen ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
        </button>
      </div>

      {isOpen && (
        <div className="mt-5 border-t border-neutral-100 pt-5">
          {isLoading && (
            <div className="py-6 text-center text-xs text-neutral-400" role="status">
              Loading history log…
            </div>
          )}

          {isError && (
            <p className="py-4 text-center text-xs text-red-600">
              Could not load version history.
            </p>
          )}

          {!isLoading && versions && (
            <div className="relative pl-6 before:absolute before:bottom-3 before:left-2.5 before:top-3 before:w-px before:bg-neutral-200">
              <div className="space-y-5">
                {versions.map((v) => (
                  <div key={v.version} className="relative">
                    {/* Timeline bullet */}
                    <div
                      className={`
                        absolute -left-6 top-1 h-2.5 w-2.5 rounded-full border-2 bg-white
                        ${v.isCurrent ? "border-neutral-900 ring-2 ring-neutral-200" : "border-neutral-400"}
                      `}
                      aria-hidden="true"
                    />

                    <div
                      className={`
                        rounded-lg border p-3.5 text-xs transition-colors
                        ${v.isCurrent ? "border-neutral-300 bg-neutral-50/50" : "border-neutral-200 bg-white"}
                      `}
                    >
                      <div className="flex flex-wrap items-center justify-between gap-2 border-b border-neutral-100 pb-2.5">
                        <div className="flex items-center gap-2">
                          <span
                            className={`
                              rounded px-1.5 py-0.5 font-mono text-[11px] font-semibold
                              ${v.isCurrent ? "bg-neutral-900 text-white" : "bg-neutral-100 text-neutral-700"}
                            `}
                          >
                            v{v.version}
                          </span>
                          {v.isCurrent && (
                            <span className="text-[11px] font-medium text-neutral-500">
                              (Current Version)
                            </span>
                          )}
                          <span className="font-medium text-neutral-800">
                            {v.modelName}
                          </span>
                        </div>
                        <span className="text-[11px] text-neutral-400">
                          {formatDateTime(v.recordedAt)}
                        </span>
                      </div>

                      <div className="mt-2.5 grid grid-cols-2 gap-2 text-[11px] sm:grid-cols-4">
                        <div>
                          <p className="text-neutral-400">Batch Number</p>
                          <p className="mt-0.5 font-mono font-medium text-neutral-800">
                            {v.batchNumber}
                          </p>
                        </div>
                        <div>
                          <p className="text-neutral-400">Production Place</p>
                          <p className="mt-0.5 font-medium text-neutral-800">
                            {v.productionPlace}
                          </p>
                        </div>
                        <div>
                          <p className="text-neutral-400">Composition</p>
                          <p className="mt-0.5 text-neutral-800">
                            {formatDecimal(v.cottonPercentage, "%", 0)} cotton /{" "}
                            {formatDecimal(v.elastanePercentage, "%", 0)} elastane
                          </p>
                        </div>
                        <div>
                          <p className="text-neutral-400">Carbon Footprint</p>
                          <p className="mt-0.5 font-mono text-neutral-800">
                            {authorised ? (
                              formatDecimal(v.carbonKgCO2e, "kg CO₂e", 2)
                            ) : (
                              <span className="inline-flex items-center gap-1 text-neutral-400">
                                <Lock size={10} aria-hidden="true" />
                                Restricted
                              </span>
                            )}
                          </p>
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

