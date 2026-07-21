import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

/** Ghép class CSS và xử lý xung đột Tailwind cho component dùng chung. */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
