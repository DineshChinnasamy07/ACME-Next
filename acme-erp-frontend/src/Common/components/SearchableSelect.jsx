import { useEffect, useId, useLayoutEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';

// Bold the part of the label that matches what the user typed.
function Highlight({ text, query }) {
    const term = query.trim();
    const index = term ? text.toLowerCase().indexOf(term.toLowerCase()) : -1;
    if (index < 0) {
        return text;
    }
    return (<>
      {text.slice(0, index)}
      <mark className="combo-match">{text.slice(index, index + term.length)}</mark>
      {text.slice(index + term.length)}
    </>);
}

// Type-to-search dropdown. The list always opens below the field. It is rendered on document.body
// (portal) so a popup's overflow / transform cannot clip or shift it.
export function SearchableSelect({ id, label, value, options, onChange, placeholder = 'Search and select', error, disabled = false, required = false, }) {
    const [open, setOpen] = useState(false);
    const [query, setQuery] = useState('');
    const [highlight, setHighlight] = useState(0);
    const [place, setPlace] = useState(null);
    const containerRef = useRef(null);
    const inputRef = useRef(null);
    const listRef = useRef(null);
    const listId = useId();
    const errorId = `${id}-error`;
    const selected = options.find((option) => String(option.value) === String(value));
    const filtered = options.filter((option) => option.label.toLowerCase().includes(query.trim().toLowerCase()));

    // Start on the selected option when the list opens; first match while typing.
    useEffect(() => {
        if (!open) {
            return;
        }
        if (query) {
            setHighlight(0);
            return;
        }
        const index = filtered.findIndex((option) => String(option.value) === String(value));
        setHighlight(index >= 0 ? index : 0);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [query, open]);

    // Close on outside click.
    useEffect(() => {
        function onDocumentMouseDown(event) {
            const target = event.target;
            if (target instanceof Node && !containerRef.current?.contains(target) && !listRef.current?.contains(target)) {
                setOpen(false);
                setQuery('');
            }
        }
        document.addEventListener('mousedown', onDocumentMouseDown);
        return () => document.removeEventListener('mousedown', onDocumentMouseDown);
    }, []);

    // Place the list under the input (fixed position), and follow it on scroll / resize.
    useLayoutEffect(() => {
        if (!open) {
            return undefined;
        }
        function measure() {
            const input = inputRef.current;
            if (!input) {
                return;
            }
            const rect = input.getBoundingClientRect();
            const below = window.innerHeight - rect.bottom - 12;
            setPlace({ top: rect.bottom + 2, left: rect.left, width: rect.width, maxHeight: Math.max(140, Math.min(280, below)) });
        }
        measure();
        window.addEventListener('resize', measure);
        window.addEventListener('scroll', measure, true);
        return () => {
            window.removeEventListener('resize', measure);
            window.removeEventListener('scroll', measure, true);
        };
    }, [open]);

    // Keep the highlighted option visible while using the arrow keys.
    useEffect(() => {
        if (!open || !listRef.current) {
            return;
        }
        const node = listRef.current.querySelector(`[data-index="${highlight}"]`);
        if (node && typeof node.scrollIntoView === 'function') {
            node.scrollIntoView({ block: 'nearest' });
        }
    }, [highlight, open]);

    function selectOption(nextValue) {
        onChange(nextValue);
        setOpen(false);
        setQuery('');
    }

    function onKeyDown(event) {
        if (event.key === 'ArrowDown') {
            event.preventDefault();
            if (!open) {
                setOpen(true);
                return;
            }
            setHighlight((current) => Math.min(current + 1, Math.max(filtered.length - 1, 0)));
            return;
        }
        if (event.key === 'ArrowUp') {
            event.preventDefault();
            setHighlight((current) => Math.max(current - 1, 0));
            return;
        }
        if (event.key === 'Enter' && open) {
            event.preventDefault();
            const option = filtered[highlight];
            if (option) {
                selectOption(option.value);
            }
            return;
        }
        if (event.key === 'Escape' && open) {
            event.stopPropagation();
            setOpen(false);
            setQuery('');
            return;
        }
        if (event.key === 'Tab') {
            setOpen(false);
            setQuery('');
        }
    }

    return (<div className="field" ref={containerRef}>
      <label htmlFor={id}>
        {label}
        {required ? (<span className="req" aria-hidden="true">
            *
          </span>) : null}
      </label>
      <div className={open ? 'combo is-open' : 'combo'}>
        <input ref={inputRef} id={id} className={error ? 'control combo-input is-invalid' : 'control combo-input'} role="combobox" autoComplete="off" aria-expanded={open} aria-controls={listId} aria-autocomplete="list" aria-activedescendant={open && filtered[highlight] ? `${listId}-${highlight}` : undefined} aria-invalid={error ? true : undefined} aria-describedby={error ? errorId : undefined} disabled={disabled} placeholder={open && selected ? selected.label : placeholder} value={open ? query : (selected?.label ?? '')} onChange={(event) => {
            setQuery(event.target.value);
            setOpen(true);
        }} onMouseDown={() => {
            if (!disabled) {
                setOpen(true);
            }
        }} onFocus={() => {
            if (!disabled) {
                setQuery('');
                setOpen(true);
            }
        }} onKeyDown={onKeyDown}/>
        <span className="combo-arrow" aria-hidden="true">▾</span>
        {open && !disabled && place ? createPortal(<ul ref={listRef} className="combo-list is-floating" id={listId} role="listbox" style={{ top: place.top, left: place.left, width: place.width, maxHeight: place.maxHeight }}>
            {filtered.map((option, index) => {
                const isSelected = String(option.value) === String(value);
                const classes = ['combo-option', index === highlight ? 'is-active' : '', isSelected ? 'is-selected' : ''].filter(Boolean).join(' ');
                return (<li key={option.value} role="presentation">
                  <button type="button" role="option" tabIndex={-1} id={`${listId}-${index}`} data-index={index} className={classes} aria-selected={isSelected} onMouseDown={(event) => event.preventDefault()} onMouseEnter={() => setHighlight(index)} onClick={() => selectOption(option.value)}>
                    <span className="combo-option-text"><Highlight text={option.label} query={query}/></span>
                    {isSelected ? <span className="combo-check" aria-hidden="true">✓</span> : null}
                  </button>
                </li>);
            })}
            {filtered.length === 0 ? <li className="combo-empty">No matches for “{query.trim()}”</li> : null}
          </ul>, document.body) : null}
      </div>
      {error ? (<p className="field-error" id={errorId}>
          {error}
        </p>) : null}
    </div>);
}
