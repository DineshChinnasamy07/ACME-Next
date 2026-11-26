import { Modal } from '../../../Common/components';

// Modal with a form, an error line, and Save / Cancel buttons.
export function FormModal({ title, isOpen, busy = false, error = '', onClose, onSave, saveLabel = 'Save', wide = false, children }) {
    return (<Modal title={title} isOpen={isOpen} busy={busy} onClose={onClose} showClose className={wide ? 'lg-modal is-wide' : 'lg-modal'} footer={<>
        <button type="button" className="lg-btn" disabled={busy} onClick={onClose}>Cancel</button>
        <button type="submit" form="lg-form" className="lg-btn is-primary" disabled={busy}>{busy ? 'Saving…' : saveLabel}</button>
      </>}>
      <form id="lg-form" className="lg-form" noValidate onSubmit={(event) => { event.preventDefault(); onSave(); }}>
        <p className="lg-form-hint">Fields marked <span className="req">*</span> are required.</p>
        {error ? <p className="lg-form-error" role="alert">{error}</p> : null}
        {children}
      </form>
    </Modal>);
}

// Two-column field grid inside a form; a section heading spans both columns.
export function FieldGrid({ children }) {
    return <div className="lg-fields">{children}</div>;
}

export function FieldSection({ title }) {
    return <h3 className="lg-section">{title}</h3>;
}

// Step-by-step form (wizard): one step's fields at a time, "Step n of N" in the footer,
// Previous / Next in the footer and Save only on the last step. The page validates each
// step in onNext (return false to stay) and the whole form in onSave.
export function StepFormModal({ title, isOpen, busy = false, error = '', steps, step, onStep, onNext, onClose, onSave, saveLabel = 'Save', children }) {
    const last = step === steps.length - 1;
    function next() {
        if (onNext(step) !== false) {
            onStep(step + 1);
        }
    }
    return (<Modal title={title} isOpen={isOpen} busy={busy} onClose={onClose} showClose className="lg-modal is-wide is-steps" footer={<>
        <button type="button" className="lg-btn" disabled={busy} onClick={onClose}>Cancel</button>
        <span className="lg-step-count" aria-live="polite">Step {step + 1} of {steps.length} · <strong>{steps[step]}</strong></span>
        <button type="button" className="lg-btn" disabled={busy || step === 0} onClick={() => onStep(step - 1)}>
          <span aria-hidden="true">&larr;</span> Previous
        </button>
        {last
          ? <button type="submit" form="lg-form" className="lg-btn is-primary" disabled={busy}>{busy ? 'Saving…' : saveLabel}</button>
          : <button type="submit" form="lg-form" className="lg-btn is-primary" disabled={busy}>Next <span aria-hidden="true">&rarr;</span></button>}
      </>}>
      <form id="lg-form" className="lg-form" noValidate onSubmit={(event) => { event.preventDefault(); if (last) { onSave(); } else { next(); } }}>
        {error ? <p className="lg-form-error" role="alert">{error}</p> : null}
        <div className="lg-step-panel">
          {children}
        </div>
        <p className="lg-form-hint">Fields marked <span className="req">*</span> are required.</p>
      </form>
    </Modal>);
}
