"use client";

import { Laptop, Moon, Sun } from "lucide-react";
import { useTheme } from "next-themes";
import { useEffect, useState } from "react";

/** ThemeToggle chuyển Light, Dark và System mà không lưu dữ liệu nhạy cảm. */
export function ThemeToggle() {
  const { theme, setTheme } = useTheme();
  const [mounted, setMounted] = useState(false);
  useEffect(() => setMounted(true), []);
  if (!mounted) return <button className="ui-icon-button" aria-label="Đổi giao diện"><Laptop size={18} /></button>;
  const next = theme === "light" ? "dark" : theme === "dark" ? "system" : "light";
  const Icon = theme === "light" ? Sun : theme === "dark" ? Moon : Laptop;
  return <button className="ui-icon-button" onClick={() => setTheme(next)} aria-label={`Giao diện hiện tại: ${theme}. Chuyển sang ${next}.`} title={`Theme: ${theme}`}><Icon size={18} /></button>;
}
