interface AuthToggleProps {
  authorised: boolean;
  onChange: (authorised: boolean) => void;
  disabled?: boolean;
}

export function AuthToggle({ authorised, onChange, disabled }: AuthToggleProps) {
  const id = "auth-toggle";

  return (
    <div className="flex items-center gap-3">
      <button
        id={id}
        type="button"
        role="switch"
        aria-checked={authorised}
        aria-label="Show restricted environmental data"
        disabled={disabled}
        onClick={() => onChange(!authorised)}
        className={`
          relative inline-flex h-5 w-9 shrink-0 cursor-pointer
          items-center rounded-full border-2 border-transparent
          transition-colors duration-200 ease-in-out
          focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-neutral-400
          disabled:cursor-not-allowed disabled:opacity-40
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
        htmlFor={id}
        className="cursor-pointer select-none text-sm text-neutral-500"
      >
        Authorised view
      </label>
    </div>
  );
}

