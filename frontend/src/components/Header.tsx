import { Shield, ShieldAlert } from "lucide-react";

interface HeaderProps {
  authorised: boolean;
  onAuthorisedChange: (authorised: boolean) => void;
}

export function Header({ authorised, onAuthorisedChange }: HeaderProps) {
  return (
    <header className="border-b border-neutral-200 bg-white">
      <div className="mx-auto flex h-16 max-w-5xl items-center justify-between px-4 sm:px-6">
        <div className="flex items-baseline gap-3">
          <a
            href="/"
            className="text-base font-semibold tracking-tight text-neutral-900 transition-opacity hover:opacity-80 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-neutral-900"
          >
            Mini-DPP
          </a>
          <span className="hidden text-xs text-neutral-400 sm:inline">
            Digital Product Passport
          </span>
        </div>

        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2">
            <button
              id="role-switch"
              type="button"
              role="switch"
              aria-checked={authorised}
              aria-label="Toggle ESG yetkili role access"
              onClick={() => onAuthorisedChange(!authorised)}
              className={`
                relative inline-flex h-5 w-9 shrink-0 cursor-pointer
                items-center rounded-full border-2 border-transparent
                transition-colors duration-200 ease-in-out
                focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-neutral-900
                ${authorised ? "bg-neutral-900" : "bg-neutral-200"}
              `}
            >
              <span
                aria-hidden="true"
                className={`
                  pointer-events-none inline-block h-3.5 w-3.5 rounded-full bg-white
                  shadow-sm ring-0 transition-transform duration-200 ease-in-out
                  ${authorised ? "translate-x-4" : "translate-x-0.5"}
                `}
              />
            </button>
            <label
              htmlFor="role-switch"
              className="flex cursor-pointer select-none items-center gap-1.5 text-xs font-medium text-neutral-600 hover:text-neutral-900"
            >
              {authorised ? (
                <>
                  <Shield size={13} className="text-neutral-900" aria-hidden="true" />
                  <span>Authorised View (yetkili)</span>
                </>
              ) : (
                <>
                  <ShieldAlert size={13} className="text-neutral-400" aria-hidden="true" />
                  <span>Public View</span>
                </>
              )}
            </label>
          </div>
        </div>
      </div>
    </header>
  );
}
