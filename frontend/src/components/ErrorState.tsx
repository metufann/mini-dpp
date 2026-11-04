interface ErrorStateProps {
  title?: string;
  message: string;
  onRetry?: () => void;
  retryLabel?: string;
}

export function ErrorState({
  title = "Something went wrong",
  message,
  onRetry,
  retryLabel = "Dismiss",
}: ErrorStateProps) {
  return (
    <div
      className="flex flex-col items-center justify-center py-10 text-center"
      role="alert"
    >
      <p className="text-sm font-semibold text-neutral-900">{title}</p>
      <p className="mt-1 text-xs text-neutral-500">{message}</p>
      {onRetry && (
        <button
          type="button"
          onClick={onRetry}
          className="
            mt-4 rounded-md border border-neutral-200 bg-white
            px-3 py-1.5 text-xs font-medium text-neutral-700
            transition-colors duration-150
            hover:bg-neutral-50
            focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-neutral-900
          "
        >
          {retryLabel}
        </button>
      )}
    </div>
  );
}
