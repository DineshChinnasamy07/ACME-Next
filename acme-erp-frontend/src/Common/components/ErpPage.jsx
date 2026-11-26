export function ErpPage({ title, className, children }) {
    const classes = ['erp-page', className].filter(Boolean).join(' ');
    return (
        <div className={classes}>
            {title ? <h2 className="erp-page-title">{title}</h2> : null}
            {children}
        </div>
    );
}
