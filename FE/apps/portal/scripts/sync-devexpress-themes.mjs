import { copyFile, mkdir } from "node:fs/promises";
import { createRequire } from "node:module";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const require = createRequire(import.meta.url);
const portalRoot = join(dirname(fileURLToPath(import.meta.url)), "..");
const themeDirectory = join(portalRoot, "public", "themes");

const themes = [
  ["devextreme-light.css", "devextreme/dist/css/dx.fluent.saas.light.compact.css"],
  ["devextreme-dark.css", "devextreme/dist/css/dx.fluent.saas.dark.compact.css"],
  ["analytics-light.css", "@devexpress/analytics-core/dist/css/dx-analytics.light.css"],
  ["analytics-dark.css", "@devexpress/analytics-core/dist/css/dx-analytics.dark.css"],
];

await mkdir(themeDirectory, { recursive: true });
await Promise.all(themes.map(([fileName, packagePath]) => copyFile(require.resolve(packagePath), join(themeDirectory, fileName))));
