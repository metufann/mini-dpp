import { Lock } from "lucide-react";

interface RestrictedNoticeProps {
  message?: string;
}

export function RestrictedNotice({
  message = "Environmental data requires authorised access.",
}: RestrictedNoticeProps) {
  return (
    <div className="flex items-start gap-3 rounded-md border border-neutral-200 bg-neutral-50 px-4 py-3">
      <Lock
        size={16}
        className="mt-0.5 shrink-0 text-neutral-400"
        aria-hidden="true"
      />
      <p className="text-sm text-neutral-500">{message}</p>
    </div>
  );
}

