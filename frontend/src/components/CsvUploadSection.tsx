import {
  type ChangeEvent,
  type DragEvent,
  useRef,
  useState,
} from "react";
import {
  AlertCircle,
  CheckCircle2,
  FileSpreadsheet,
  Upload,
  X,
} from "lucide-react";
import { MAX_UPLOAD_BYTES } from "../lib/constants";
import { formatFileSize } from "../lib/format";
import { useImportCsv } from "../hooks/useProducts";
import type { ImportResultDto } from "../types/product";

interface CsvUploadSectionProps {
  onImportSuccess?: () => void;
}

export function CsvUploadSection({ onImportSuccess }: CsvUploadSectionProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [fileValidationError, setFileValidationError] = useState<string | null>(null);
  const [importResult, setImportResult] = useState<ImportResultDto | null>(null);
  const [isDragOver, setIsDragOver] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const { mutate: importFile, isPending, error: uploadError, reset: resetMutation } =
    useImportCsv();

  function validateSelectedFile(file: File): boolean {
    setFileValidationError(null);

    if (!file.name.toLowerCase().endsWith(".csv")) {
      setFileValidationError("Only .csv files are supported.");
      return false;
    }

    if (file.size === 0) {
      setFileValidationError("The selected file is empty.");
      return false;
    }

    if (file.size > MAX_UPLOAD_BYTES) {
      setFileValidationError(
        `The file size exceeds the ${MAX_UPLOAD_BYTES / (1024 * 1024)} MB limit.`,
      );
      return false;
    }

    return true;
  }

  function handleFileSelect(file: File) {
    if (validateSelectedFile(file)) {
      setSelectedFile(file);
      setImportResult(null);
      resetMutation();
    }
  }

  function handleInputChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
  }

  function handleDragOver(e: DragEvent<HTMLDivElement>) {
    e.preventDefault();
    setIsDragOver(true);
  }

  function handleDragLeave(e: DragEvent<HTMLDivElement>) {
    e.preventDefault();
    setIsDragOver(false);
  }

  function handleDrop(e: DragEvent<HTMLDivElement>) {
    e.preventDefault();
    setIsDragOver(false);
    const file = e.dataTransfer.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
  }

  function handleClear() {
    setSelectedFile(null);
    setFileValidationError(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  }

  function handleUpload() {
    if (!selectedFile) return;

    importFile(selectedFile, {
      onSuccess: (data) => {
        setImportResult(data);
        handleClear();
        onImportSuccess?.();
      },
    });
  }

  return (
    <section
      aria-labelledby="csv-upload-title"
      className="rounded-xl border border-neutral-200 bg-white p-5 sm:p-6"
    >
      <div className="mb-4">
        <h2
          id="csv-upload-title"
          className="text-sm font-semibold tracking-tight text-neutral-900"
        >
          Import Products
        </h2>
        <p className="mt-1 text-xs text-neutral-500">
          Upload a UTF-8 CSV file containing product passport data (max 2 MB).
        </p>
      </div>

      {/* Dropzone */}
      <div
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onClick={() => fileInputRef.current?.click()}
        className={`
          flex cursor-pointer flex-col items-center justify-center rounded-lg border border-dashed
          p-6 text-center transition-colors duration-150
          ${
            isDragOver
              ? "border-neutral-900 bg-neutral-50"
              : "border-neutral-300 hover:border-neutral-400 hover:bg-neutral-50/50"
          }
        `}
      >
        <input
          ref={fileInputRef}
          type="file"
          accept=".csv,text/csv"
          onChange={handleInputChange}
          className="sr-only"
          aria-label="Select CSV file for upload"
        />
        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-neutral-100 text-neutral-600">
          <Upload size={18} aria-hidden="true" />
        </div>
        <p className="mt-3 text-sm font-medium text-neutral-800">
          Click to choose a file or drag and drop
        </p>
        <p className="mt-1 text-xs text-neutral-400">
          CSV format: gtin, model_adi, batch_no, elyaf_pamuk_yuzde, elyaf_elastan_yuzde, uretim_yeri, karbon_kg_co2e
        </p>
      </div>

      {/* File validation error */}
      {fileValidationError && (
        <div
          role="alert"
          className="mt-3 flex items-center gap-2 rounded-md border border-neutral-200 bg-neutral-50 px-3 py-2 text-xs text-neutral-700"
        >
          <AlertCircle size={14} className="shrink-0 text-neutral-500" />
          <span>{fileValidationError}</span>
        </div>
      )}

      {/* Selected file preview & action */}
      {selectedFile && (
        <div className="mt-4 flex flex-col gap-3 rounded-lg border border-neutral-200 bg-neutral-50/60 p-3 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex items-center gap-2.5 min-w-0">
            <FileSpreadsheet
              size={18}
              className="shrink-0 text-neutral-500"
              aria-hidden="true"
            />
            <div className="min-w-0">
              <p className="truncate text-xs font-medium text-neutral-900">
                {selectedFile.name}
              </p>
              <p className="text-[11px] text-neutral-400">
                {formatFileSize(selectedFile.size)}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                handleClear();
              }}
              disabled={isPending}
              className="rounded-md p-1.5 text-neutral-400 hover:bg-neutral-200/60 hover:text-neutral-700 disabled:opacity-50"
              aria-label="Remove selected file"
            >
              <X size={15} />
            </button>
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                handleUpload();
              }}
              disabled={isPending}
              className="
                inline-flex items-center justify-center rounded-md bg-neutral-900 px-3.5 py-1.5
                text-xs font-medium text-white transition-opacity hover:opacity-90
                focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-neutral-900
                disabled:cursor-not-allowed disabled:opacity-50
              "
            >
              {isPending ? "Uploading…" : "Upload & Import"}
            </button>
          </div>
        </div>
      )}

      {/* Upload error */}
      {uploadError && (
        <div
          role="alert"
          className="mt-3 flex items-start gap-2.5 rounded-md border border-red-200 bg-red-50/50 p-3 text-xs text-red-800"
        >
          <AlertCircle size={15} className="mt-0.5 shrink-0 text-red-600" />
          <div>
            <p className="font-medium">Upload failed</p>
            <p className="mt-0.5">{uploadError.message}</p>
          </div>
        </div>
      )}

      {/* Import results */}
      {importResult && (
        <div
          role="status"
          className="mt-4 rounded-lg border border-neutral-200 bg-neutral-50/80 p-4"
        >
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <CheckCircle2
                size={16}
                className="text-neutral-900"
                aria-hidden="true"
              />
              <span className="text-xs font-semibold tracking-tight text-neutral-900">
                Import Summary
              </span>
            </div>
            <button
              type="button"
              onClick={() => setImportResult(null)}
              className="text-xs text-neutral-400 hover:text-neutral-700"
            >
              Dismiss
            </button>
          </div>

          {/* Metrics summary */}
          <div className="mt-3 grid grid-cols-3 gap-2 border-y border-neutral-200 py-3 text-center sm:gap-4">
            <div>
              <p className="text-[11px] text-neutral-400">Total Rows</p>
              <p className="mt-0.5 font-mono text-sm font-semibold text-neutral-900">
                {importResult.totalRows}
              </p>
            </div>
            <div>
              <p className="text-[11px] text-neutral-400">Saved Rows</p>
              <p className="mt-0.5 font-mono text-sm font-semibold text-neutral-900">
                {importResult.savedRows}
              </p>
            </div>
            <div>
              <p className="text-[11px] text-neutral-400">Failed Rows</p>
              <p className="mt-0.5 font-mono text-sm font-semibold text-neutral-900">
                {importResult.failedRows}
              </p>
            </div>
          </div>

          {/* Sub-breakdown: Created vs Updated */}
          {(importResult.createdRows !== undefined || importResult.updatedRows !== undefined) && (
            <div className="mt-2 flex flex-wrap items-center justify-center gap-3 text-[11px] text-neutral-600">
              {importResult.createdRows !== undefined && (
                <span>
                  New: <strong className="font-mono text-neutral-900">{importResult.createdRows}</strong>
                </span>
              )}
              {importResult.updatedRows !== undefined && (
                <span>
                  Updated: <strong className="font-mono text-neutral-900">{importResult.updatedRows}</strong>
                </span>
              )}
              {importResult.unchangedRows !== undefined && importResult.unchangedRows > 0 && (
                <span className="text-neutral-400">
                  Unchanged: <strong className="font-mono">{importResult.unchangedRows}</strong>
                </span>
              )}
            </div>
          )}

          {/* Partial success notice */}
          {importResult.savedRows > 0 && importResult.failedRows > 0 && (
            <p className="mt-2.5 text-xs text-neutral-600">
              Valid rows were saved successfully. Invalid rows are detailed below:
            </p>
          )}

          {/* Error detail list */}
          {importResult.errors.length > 0 && (
            <div className="mt-3">
              <p className="text-[11px] font-semibold text-neutral-600 uppercase tracking-wider">
                Row-Level Issues ({importResult.errors.length})
              </p>
              <ul className="mt-2 divide-y divide-neutral-200 text-xs">
                {importResult.errors.map((err, idx) => (
                  <li
                    key={`${err.row}-${idx}`}
                    className="flex flex-col gap-0.5 py-1.5 sm:flex-row sm:gap-3"
                  >
                    <span className="shrink-0 font-mono font-medium text-neutral-500 sm:w-16">
                      Row {err.row}
                    </span>
                    <span className="text-neutral-800">
                      {err.reasons.join(", ")}
                    </span>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}
    </section>
  );
}

