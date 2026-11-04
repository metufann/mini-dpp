export function LoadingState() {
  return (
    <div
      className="flex flex-col items-center justify-center py-16"
      role="status"
      aria-label="Loading product data"
    >
      <div
        className="h-6 w-6 animate-spin rounded-full border-2 border-neutral-200 border-t-neutral-600"
        style={{ animationDuration: "0.7s" }}
      />
      <p className="mt-4 text-sm text-neutral-400">
        Looking up product…
      </p>
    </div>
  );
}

