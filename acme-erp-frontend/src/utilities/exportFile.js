/*
 * List export helpers shared by list pages.
 * columns: [{ header, value(row) }]
 */
function downloadFile(filename, content, type) {
    const blob = new Blob([content], { type });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(url);
}

function csvCell(value) {
    const text = String(value == null ? '' : value);
    if (/[",\n]/.test(text)) {
        return `"${text.replace(/"/g, '""')}"`;
    }
    return text;
}

function pdfText(value) {
    return String(value == null ? '' : value).replace(/[^\x20-\x7E]/g, '?').replace(/\\/g, '\\\\').replace(/\(/g, '\\(').replace(/\)/g, '\\)');
}

export function exportExcel(filename, columns, rows) {
    const header = columns.map((column) => column.header).join(',');
    const body = rows.map((row) => columns.map((column) => csvCell(column.value(row))).join(','));
    downloadFile(filename, [header, ...body].join('\n'), 'text/csv;charset=utf-8');
}

export function exportPdf(filename, title, columns, rows) {
    const lines = [title, columns.map((column) => column.header).join(' | ')];
    rows.forEach((row) => lines.push(columns.map((column) => column.value(row)).join(' | ')));
    const commands = ['BT', '/F1 10 Tf', '40 760 Td', '14 TL'];
    lines.slice(0, 48).forEach((line, index) => {
        const text = `(${pdfText(line).slice(0, 100)}) Tj`;
        commands.push(index === 0 ? text : `T* ${text}`);
    });
    commands.push('ET');
    const stream = commands.join('\n');
    const objects = [
        '1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj',
        '2 0 obj << /Type /Pages /Count 1 /Kids [3 0 R] >> endobj',
        '3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >> endobj',
        `4 0 obj << /Length ${stream.length} >> stream\n${stream}\nendstream endobj`,
        '5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj',
    ];
    let body = '%PDF-1.4\n';
    const offsets = [];
    objects.forEach((object) => {
        offsets.push(body.length);
        body += `${object}\n`;
    });
    const xref = body.length;
    body += `xref\n0 ${objects.length + 1}\n0000000000 65535 f \n`;
    offsets.forEach((offset) => {
        body += `${String(offset).padStart(10, '0')} 00000 n \n`;
    });
    body += `trailer << /Size ${objects.length + 1} /Root 1 0 R >>\nstartxref\n${xref}\n%%EOF`;
    downloadFile(filename, body, 'application/pdf');
}
