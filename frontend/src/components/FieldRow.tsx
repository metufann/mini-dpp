interface FieldRowProps {
  label: string;
  value: string | number | null | undefined;
  mono?: boolean;
}

export function FieldRow({ label, value, mono }: FieldRowProps) {
  const displayValue = value == null || value === "" ? "—" : String(value);
  return (
    <div className="flex flex-col gap-0.5 py-2.5 sm:flex-row sm:gap-4">
      <dt className="shrink-0 text-sm text-neutral-500 sm:w-44">{label}</dt>
      <dd
        className={`text-sm text-neutral-900 break-words ${mono ? "font-mono tracking-tight" : ""}`}
      >
        {displayValue}
      </dd>
    </div>
  );
}

