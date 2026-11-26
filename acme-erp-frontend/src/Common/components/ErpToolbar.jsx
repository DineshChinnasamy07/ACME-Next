export function ErpToolbar({ filters, actions, children }) {
    return (
        <div className="erp-toolbar">
            {filters ? <div className="erp-toolbar-filters">{filters}</div> : <div className="erp-toolbar-filters" />}
            <div className="erp-toolbar-actions">
                {actions}
                {children}
            </div>
        </div>
    );
}
