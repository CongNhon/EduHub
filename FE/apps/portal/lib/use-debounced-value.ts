"use client";

import { useEffect, useState } from "react";

/** useDebouncedValue trì hoãn server-side search để không gửi request sau từng phím bấm. */
export function useDebouncedValue<T>(value: T, delayMs = 300) {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const timer = window.setTimeout(() => setDebounced(value), delayMs);
    return () => window.clearTimeout(timer);
  }, [delayMs, value]);
  return debounced;
}
