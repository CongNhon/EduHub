import type { ReactNode } from "react";

export interface DataColumn<T> {
  key: string;
  header: string;
  cell: (row: T) => ReactNode;
  className?: string;
}

/** DataTable hiển thị danh sách nghiệp vụ với header ổn định và empty state. */
export function DataTable<T>({ columns, rows, rowKey, empty }: { columns: DataColumn<T>[]; rows: T[]; rowKey: (row: T) => string; empty?: ReactNode }) {
  return (
    <div className="ui-table-wrap">
      <table className="ui-table">
        <thead><tr>{columns.map((column) => <th key={column.key} className={column.className}>{column.header}</th>)}</tr></thead>
        <tbody>
          {rows.map((row) => <tr key={rowKey(row)}>{columns.map((column) => <td key={column.key} className={column.className}>{column.cell(row)}</td>)}</tr>)}
        </tbody>
      </table>
      {rows.length === 0 ? <div className="ui-table__empty">{empty || "Chưa có dữ liệu."}</div> : null}
    </div>
  );
}
