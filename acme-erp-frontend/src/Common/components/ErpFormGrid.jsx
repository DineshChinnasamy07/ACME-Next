export function ErpFormGrid({ columns = 2, className, children }) {
    const columnClass = columns === 1 ? 'erp-form-grid-1' : 'erp-form-grid-2';
    const classes = ['erp-form-grid', columnClass, className].filter(Boolean).join(' ');
    return <div className={classes}>{children}</div>;
}
