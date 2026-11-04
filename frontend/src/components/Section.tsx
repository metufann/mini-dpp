import type { ReactNode } from "react";

interface SectionProps {
  title: string;
  children: ReactNode;
}

export function Section({ title, children }: SectionProps) {
  return (
    <section className="py-6 first:pt-0">
      <h2 className="mb-3 text-xs font-semibold tracking-widest text-neutral-400 uppercase">
        {title}
      </h2>
      <dl className="divide-y divide-neutral-100">{children}</dl>
    </section>
  );
}

