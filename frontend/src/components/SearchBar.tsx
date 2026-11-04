import { type FormEvent, type KeyboardEvent, useRef, useState } from "react";
import { Search, X } from "lucide-react";
import { validateGtin } from "../lib/gtin";

interface SearchBarProps {
  onSearch: (gtin: string) => void;
  isLoading: boolean;
}

export function SearchBar({ onSearch, isLoading }: SearchBarProps) {
  const [value, setValue] = useState("");
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  function submit() {
    const trimmed = value.trim();
    const validation = validateGtin(trimmed);
    if (!validation.valid) {
      setError(validation.error ?? "Invalid GTIN.");
      inputRef.current?.focus();
      return;
    }
    setError(null);
    onSearch(trimmed);
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    submit();
  }

  function handleKeyDown(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Enter") {
      e.preventDefault();
      submit();
    }
  }

  function handleClear() {
    setValue("");
    setError(null);
    inputRef.current?.focus();
  }

  return (
    <div className="w-full">
      <form onSubmit={handleSubmit} role="search" className="relative">
        <label htmlFor="gtin-input" className="sr-only">
          Search by 14-digit GTIN
        </label>
        <div className="relative flex items-center">
          <Search
            size={16}
            className="pointer-events-none absolute left-3.5 text-neutral-400"
            aria-hidden="true"
          />
          <input
            ref={inputRef}
            id="gtin-input"
            type="text"
            inputMode="numeric"
            pattern="[0-9]*"
            maxLength={14}
            autoComplete="off"
            spellCheck="false"
            placeholder="Enter 14-digit GTIN (e.g. 08690000000012)"
            value={value}
            onChange={(e) => {
              setValue(e.target.value);
              if (error) setError(null);
            }}
            onKeyDown={handleKeyDown}
            disabled={isLoading}
            className={`
              w-full rounded-lg border bg-white py-2.5 pl-10 pr-24 font-mono
              text-xs text-neutral-900 placeholder:font-sans placeholder:text-neutral-400
              outline-none transition-colors duration-150
              focus:border-neutral-900 focus:ring-1 focus:ring-neutral-900
              disabled:cursor-not-allowed disabled:opacity-60
              ${
                error
                  ? "border-red-300 focus:border-red-500 focus:ring-red-500"
                  : "border-neutral-200"
              }
            `}
            aria-invalid={error ? "true" : undefined}
            aria-describedby={error ? "gtin-error" : undefined}
          />

          <div className="absolute right-1.5 flex items-center gap-1">
            {value.length > 0 && (
              <button
                type="button"
                onClick={handleClear}
                disabled={isLoading}
                className="rounded p-1 text-neutral-400 hover:text-neutral-700 disabled:opacity-50"
                aria-label="Clear GTIN input"
              >
                <X size={14} />
              </button>
            )}
            <button
              type="submit"
              disabled={isLoading || value.trim().length === 0}
              className="
                rounded-md bg-neutral-900 px-3 py-1.5 text-xs font-medium text-white
                transition-opacity hover:opacity-90
                focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-neutral-900
                disabled:cursor-not-allowed disabled:opacity-40
              "
            >
              {isLoading ? "Searching…" : "Look up"}
            </button>
          </div>
        </div>
      </form>
      {error && (
        <p
          id="gtin-error"
          role="alert"
          className="mt-1.5 text-xs text-red-600"
        >
          {error}
        </p>
      )}
    </div>
  );
}
