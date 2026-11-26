import { useEffect, useMemo, useState } from 'react';

function compareValues(left, right) {
    if (typeof left === 'number' && typeof right === 'number') {
        return left - right;
    }
    return String(left).localeCompare(String(right), undefined, { numeric: true, sensitivity: 'base' });
}

export function DataTable({
    columns,
    rows,
    getRowKey,
    emptyMessage = 'No records to display.',
    caption,
    selectedKey,
    onRowSelect,
    pageSize = 0,
    zebra = true,
}) {
    const [sort, setSort] = useState(null);
    const [page, setPage] = useState(0);
    const [focusIndex, setFocusIndex] = useState(-1);

    const sortedRows = useMemo(() => {
        if (!sort) {
            return rows;
        }
        const column = columns.find((item) => item.key === sort.key);
        if (!column?.sortValue) {
            return rows;
        }
        const next = [...rows].sort((left, right) => compareValues(column.sortValue(left), column.sortValue(right)));
        return sort.direction === 'desc' ? next.reverse() : next;
    }, [columns, rows, sort]);

    const pageCount = pageSize > 0 ? Math.max(1, Math.ceil(sortedRows.length / pageSize) || 1) : 1;
    const safePage = Math.min(page, pageCount - 1);
    const visibleRows = pageSize > 0
        ? sortedRows.slice(safePage * pageSize, safePage * pageSize + pageSize)
        : sortedRows;

    useEffect(() => {
        setPage(0);
        setFocusIndex(-1);
    }, [rows, pageSize, sort]);

    useEffect(() => {
        if (page !== safePage) {
            setPage(safePage);
        }
    }, [page, safePage]);

    function toggleSort(column) {
        if (!column.sortValue) {
            return;
        }
        setSort((current) => {
            if (!current || current.key !== column.key) {
                return { key: column.key, direction: 'asc' };
            }
            if (current.direction === 'asc') {
                return { key: column.key, direction: 'desc' };
            }
            return null;
        });
    }

    function selectRow(row, index) {
        setFocusIndex(index);
        if (onRowSelect) {
            onRowSelect(row);
        }
    }

    function onKeyDown(event) {
        if (visibleRows.length === 0) {
            return;
        }
        if (event.key === 'ArrowDown') {
            event.preventDefault();
            const next = Math.min(visibleRows.length - 1, focusIndex < 0 ? 0 : focusIndex + 1);
            setFocusIndex(next);
            if (onRowSelect) {
                onRowSelect(visibleRows[next]);
            }
        }
        else if (event.key === 'ArrowUp') {
            event.preventDefault();
            const next = Math.max(0, focusIndex < 0 ? 0 : focusIndex - 1);
            setFocusIndex(next);
            if (onRowSelect) {
                onRowSelect(visibleRows[next]);
            }
        }
        else if (event.key === 'Enter' && focusIndex >= 0 && onRowSelect) {
            event.preventDefault();
            onRowSelect(visibleRows[focusIndex]);
        }
    }

    const tableClass = ['data-table', zebra ? 'is-zebra' : ''].filter(Boolean).join(' ');
    const from = sortedRows.length === 0 ? 0 : (pageSize > 0 ? safePage * pageSize + 1 : 1);
    const to = pageSize > 0 ? Math.min(sortedRows.length, (safePage + 1) * pageSize) : sortedRows.length;

    return (
        <div className="table-wrap">
            <table
                className={tableClass}
                tabIndex={0}
                onKeyDown={onKeyDown}
                aria-rowcount={sortedRows.length}
            >
                {caption ? <caption className="sr-only">{caption}</caption> : null}
                <thead>
                    <tr>
                        {columns.map((column) => {
                            const active = sort?.key === column.key ? sort.direction : null;
                            return (
                                <th key={column.key} scope="col" aria-sort={active === 'asc' ? 'ascending' : active === 'desc' ? 'descending' : 'none'}>
                                    {column.sortValue ? (
                                        <button type="button" className="sort-button" onClick={() => toggleSort(column)}>
                                            {column.header}
                                            <span aria-hidden="true">{active === 'asc' ? '↑' : active === 'desc' ? '↓' : '↕'}</span>
                                        </button>
                                    ) : column.header}
                                </th>
                            );
                        })}
                    </tr>
                </thead>
                <tbody>
                    {visibleRows.length === 0 ? (
                        <tr>
                            <td className="table-empty" colSpan={columns.length}>
                                {emptyMessage}
                            </td>
                        </tr>
                    ) : visibleRows.map((row, index) => {
                        const key = getRowKey(row);
                        const selected = selectedKey === key;
                        const focused = focusIndex === index;
                        const rowClass = [
                            selected ? 'is-selected' : '',
                            focused ? 'is-focused' : '',
                            onRowSelect ? 'is-selectable' : '',
                        ].filter(Boolean).join(' ') || undefined;
                        return (
                            <tr
                                key={key}
                                className={rowClass}
                                onClick={() => selectRow(row, index)}
                            >
                                {columns.map((column) => (
                                    <td key={column.key}>{column.render(row)}</td>
                                ))}
                            </tr>
                        );
                    })}
                </tbody>
            </table>
            {pageSize > 0 && sortedRows.length > 0 ? (
                <div className="table-pager">
                    <span>{from}–{to} of {sortedRows.length}</span>
                    <button
                        type="button"
                        className="btn btn-outline btn-small"
                        disabled={safePage <= 0}
                        onClick={() => setPage((current) => Math.max(0, current - 1))}
                    >
                        Prev
                    </button>
                    <button
                        type="button"
                        className="btn btn-outline btn-small"
                        disabled={safePage >= pageCount - 1}
                        onClick={() => setPage((current) => Math.min(pageCount - 1, current + 1))}
                    >
                        Next
                    </button>
                </div>
            ) : null}
        </div>
    );
}
