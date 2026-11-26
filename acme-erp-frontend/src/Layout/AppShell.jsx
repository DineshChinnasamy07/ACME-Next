import { useEffect, useMemo, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useAuth } from '../Context/AuthContext';
import { brand } from '../constants/brand';
import acmeLogo from '../Assets/images/acme-logo.svg';
import { ProjectPage } from '../Modules/Data/pages/ProjectPage';
import { LegalEntityPage } from '../Modules/Data/pages/LegalEntityPage';
import { GoverningBodiesPage } from '../Modules/Data/pages/GoverningBodiesPage';
import { MapGoverningBodiesPage } from '../Modules/Data/pages/MapGoverningBodiesPage';
import { TdsSectionPage } from '../Modules/Data/pages/TdsSectionPage';
import { NatureOfPaymentsPage } from '../Modules/Data/pages/NatureOfPaymentsPage';
import { DeducteeTypePage } from '../Modules/Data/pages/DeducteeTypePage';
import { TdsPolicyPage } from '../Modules/Data/pages/TdsPolicyPage';
import { DutyTaxPage } from '../Modules/Data/pages/DutyTaxPage';
import { LedgerPage } from '../Modules/Data/pages/LedgerPage';
import { LedgerGroupPage } from '../Modules/Data/pages/LedgerGroupPage';
import { AccountingPeriodPage } from '../Modules/Data/pages/AccountingPeriodPage';
import { LockVoucherPage } from '../Modules/Data/pages/LockVoucherPage';
import { ProjectCategoryPage } from '../Modules/Data/pages/ProjectCategoryPage';
import { FcPurposePage } from '../Modules/Data/pages/FcPurposePage';
import { GeneralateLedgerGroupPage } from '../Modules/Data/pages/GeneralateLedgerGroupPage';
import { BranchLocationPage } from '../Modules/Office/pages/BranchLocationPage';
import { BranchOfficePage } from '../Modules/Office/pages/BranchOfficePage';
import { HeadOfficePage } from '../Modules/Office/pages/HeadOfficePage';
import { ChangePasswordPage } from '../Modules/User/pages/ChangePasswordPage';
import { ProjectMappingPage } from '../Modules/Data/pages/ProjectMappingPage';
import { UserListPage } from '../Modules/User/pages/UserListPage';
import { UserRightsPage } from '../Modules/User/pages/UserRightsPage';
import { UserRolePage } from '../Modules/User/pages/UserRolePage';
import { isBranchSession, isPortalSession } from '../Modules/Common/services/apiRequest';
import { branchOfficeMenu, findMenuPath, headOfficeMenu, siteAdminMenu } from './menus';

// Screens behind each menu item; the others show their title until built.
const screens = {
    'user-list': UserListPage,
    'user-role': UserRolePage,
    'user-rights': UserRightsPage,
    'change-password': ChangePasswordPage,
    'head-office': HeadOfficePage,
    'branch-office': BranchOfficePage,
    'office.branchlocationview': BranchLocationPage,
    'data.accountingperiod': AccountingPeriodPage,
    'data.ledgergroupview': LedgerGroupPage,
    'data.ledgerview': LedgerPage,
    'data.projectview': ProjectPage,
    'data.projectmapping': ProjectMappingPage,
    'data.lockvoucherview': LockVoucherPage,
    'data.projectcategoryview': ProjectCategoryPage,
    'data.fcpurposeview': FcPurposePage,
    'data.generalateledgergroup': GeneralateLedgerGroupPage,
    'data.legalentityview': LegalEntityPage,
    'data.governingmemberview': GoverningBodiesPage,
    'data.mapgoverningbody': MapGoverningBodiesPage,
    'data.tdssectionview': TdsSectionPage,
    'data.tdspaymentnatureview': NatureOfPaymentsPage,
    'data.tdsdeducteetypeview': DeducteeTypePage,
    'data.tdspolicyview': TdsPolicyPage,
    'data.tdsdutytaxview': DutyTaxPage,
};

// One dropdown level; a child with its own children opens a fly-out to the right on hover / focus.
function SubMenu({ items, label, open, selectedPath, onChoose, nested = false }) {
    const onPath = (item) => selectedPath.some((node) => node.id === item.id);
    return (<div className={nested ? 'legacy-submenu is-nested' : 'legacy-submenu'} role="menu" aria-label={label}>
      {items.map((item) => (item.children
        ? (<div key={item.id} className="legacy-submenu-group">
            <button type="button" role="menuitem" aria-haspopup="true" className={onPath(item) ? 'legacy-submenu-item has-children is-active' : 'legacy-submenu-item has-children'} tabIndex={open ? 0 : -1}>
              {item.title}
              <span className="legacy-submenu-arrow" aria-hidden="true">›</span>
            </button>
            <SubMenu items={item.children} label={item.title} open={open} selectedPath={selectedPath} onChoose={onChoose} nested/>
          </div>)
        : (<button key={item.id} type="button" role="menuitem" className={onPath(item) ? 'legacy-submenu-item is-active' : 'legacy-submenu-item'} tabIndex={open ? 0 : -1} onClick={() => onChoose(item.id)}>
            {item.title}
          </button>)))}
    </div>);
}

// Left sidebar for the branch login — bank-portal style (burgundy accents, Lato): rounded search,
// accordion sections with chevrons, items with hover/active pink pill and burgundy bar.
function SideMenu({ module, selectedPath, openGroups, onToggle, onChoose, search, onSearch }) {
    const onPath = (item) => selectedPath.some((node) => node.id === item.id);
    const term = search.trim().toLowerCase();
    const sections = module.children
        .map((section) => ({ section, items: (section.children || []).filter((item) => !term || item.title.toLowerCase().includes(term)) }))
        .filter(({ section, items }) => !term || items.length > 0 || section.title.toLowerCase().includes(term));
    return (<>
      <div className="ax-side-head">
        <span className="ax-side-kicker">Menu</span>
        <h2 className="ax-side-title">{module.title}</h2>
        <label className="ax-search">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M10 3a7 7 0 0 1 5.6 11.2l4.6 4.6-1.4 1.4-4.6-4.6A7 7 0 1 1 10 3zm0 2a5 5 0 1 0 0 10 5 5 0 0 0 0-10z"/></svg>
          <input type="search" placeholder="What are you looking for?" aria-label={`Search ${module.title} menu`} value={search} onChange={(event) => onSearch(event.target.value)}/>
        </label>
      </div>
      <nav className="ax-side-sections">
        {sections.length === 0 ? <p className="ax-side-empty">No menu matches “{search.trim()}”.</p> : null}
        {sections.map(({ section, items }) => {
            const open = Boolean(term) || (openGroups[section.id] ?? onPath(section));
            return (<section key={section.id} className={`ax-section${open ? ' is-open' : ''}${onPath(section) ? ' is-path' : ''}`}>
              <button type="button" className="ax-section-head" aria-expanded={open} onClick={() => onToggle(section.id, !open)}>
                <span className="ax-section-title">{section.title}</span>
                <span className="ax-section-count">{(section.children || []).length}</span>
                <svg className="ax-chevron" viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M7.4 8.6 12 13.2l4.6-4.6L18 10l-6 6-6-6z"/></svg>
              </button>
              {open ? (<ul className="ax-items">
                {items.map((item) => (<li key={item.id}>
                  <button type="button" className={onPath(item) ? 'ax-item is-active' : 'ax-item'} aria-current={onPath(item) ? 'page' : undefined} onClick={() => onChoose(item.id)}>
                    {item.title}
                  </button>
                </li>))}
              </ul>) : null}
            </section>);
        })}
      </nav>
    </>);
}

// Legacy-style page frame: header, horizontal menu, content, footer.
// children = the Home page; other menu items show their title until their pages are built.
export function AppShell({ children }) {
    const { session, logout } = useAuth();
    const [searchParams, setSearchParams] = useSearchParams();
    const userName = session ? (session.userName || session.displayName || 'User') : 'User';
    const [openId, setOpenId] = useState('');
    const menuRef = useRef(null);
    // Portal login → Site Admin menu; head office login → Head Office menu (legacy sitemaps).
    // Branch office login → Branch Office menu (Home / Finance / Utilities), same layout.
    const menu = useMemo(() => {
        if (isPortalSession(session)) {
            return siteAdminMenu;
        }
        return isBranchSession(session) ? branchOfficeMenu : headOfficeMenu;
    }, [session]);
    // The open screen lives in the URL (?page=<menu id>) so a browser refresh, Back/Forward
    // and bookmarks keep the same page. Unknown ids (old links, other user's menu) fall back to Home.
    const requestedId = searchParams.get('page') || 'home';
    const selectedId = requestedId === 'account' || findMenuPath(menu, requestedId) ? requestedId : 'home';
    // Head office login shows that head office's name; the portal keeps the product title.
    const headerTitle = !session || isPortalSession(session)
        ? 'ERP for Religious & NGOs'
        : (session.headOfficeName || session.headOfficeId || 'ERP for Religious & NGOs');
    const selectedPath = findMenuPath(menu, selectedId) || [];
    const selectedTop = selectedPath.length > 0 ? selectedPath[0] : null;
    const Screen = screens[selectedId];
    const selectedTitle = selectedPath.length > 0 ? selectedPath[selectedPath.length - 1].title : 'My Account';
    // Branch login: top items open a left sidebar with that module's menu (no dropdowns).
    const sideMode = isBranchSession(session);
    const sideModule = sideMode && selectedTop && selectedTop.children ? selectedTop : null;
    const [openSideGroups, setOpenSideGroups] = useState({});
    const [sideSearch, setSideSearch] = useState('');

    // Close an open dropdown on outside click or Escape.
    useEffect(() => {
        if (!openId) {
            return undefined;
        }
        function onPointerDown(event) {
            if (menuRef.current && !menuRef.current.contains(event.target)) {
                setOpenId('');
            }
        }
        function onKeyDown(event) {
            if (event.key === 'Escape') {
                setOpenId('');
            }
        }
        document.addEventListener('mousedown', onPointerDown);
        document.addEventListener('keydown', onKeyDown);
        return () => {
            document.removeEventListener('mousedown', onPointerDown);
            document.removeEventListener('keydown', onKeyDown);
        };
    }, [openId]);

    function choose(id) {
        setSearchParams(id === 'home' ? {} : { page: id });
        setOpenId('');
    }

    return (<div className={sideMode ? 'legacy-page is-axis' : 'legacy-page'}>
      <div className="legacy-frame">
        {sideMode ? (<header className="ax-header">
          <div className="ax-brand">
            <img src={acmeLogo} alt=""/>
            <span className="ax-brand-name">ACME ERP</span>
          </div>
          <div className="ax-header-main">
            <div className="ax-topbar">
              <span className="ax-org-pill" title={headerTitle}>{headerTitle}</span>
              {session.branchOfficeId && session.branchOfficeId !== 'portal'
                ? <span className="ax-top-link">Branch : {String(session.branchOfficeId).toUpperCase()}</span>
                : null}
              <span className="ax-top-spacer"/>
              <span className="ax-top-link">Welcome, <strong>{userName}</strong></span>
              <button type="button" className="ax-top-link is-button" onClick={() => choose('account')}>My Account</button>
              <button type="button" className="ax-logout" onClick={() => void logout()}>Logout</button>
            </div>
            <nav className="ax-nav" aria-label="Main menu">
              {menu.map((item) => {
                  const active = selectedTop && selectedTop.id === item.id;
                  return (<button key={item.id} type="button" className={active ? 'ax-nav-item is-active' : 'ax-nav-item'} aria-current={active ? 'page' : undefined} onClick={() => choose(item.id)}>
                    {item.title}
                    {item.children ? <svg className="ax-chevron" viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M7.4 8.6 12 13.2l4.6-4.6L18 10l-6 6-6-6z"/></svg> : null}
                  </button>);
              })}
            </nav>
          </div>
        </header>) : (<>
        <header className="legacy-header">
          <img className="legacy-logo" src={acmeLogo} alt="Acme.erp"/>
          <div className="legacy-title" title={headerTitle}>{headerTitle}</div>
          <div className="legacy-header-right">
            <div className="legacy-vendor" aria-label="boscosoft, Innovate and Lead">
              <span className="legacy-vendor-name">boscosoft</span>
              <span className="legacy-vendor-mark" aria-hidden="true"/>
              <span className="legacy-vendor-tag">Innovate and Lead</span>
            </div>
            <div className="legacy-welcome">
              Welcome {userName}! [
              <button type="button" className="legacy-link" onClick={() => choose('account')}>My Account</button>
              <span className="legacy-pipe">|</span>
              <button type="button" className="legacy-link" onClick={() => void logout()}>Logout</button>
              ]
            </div>
          </div>
        </header>

        <nav className="legacy-menu" aria-label="Main menu" ref={menuRef}>
          {menu.map((item) => {
              const active = selectedTop && selectedTop.id === item.id;
              if (!item.children) {
                  return (<button key={item.id} type="button" className={active ? 'legacy-menu-item is-active' : 'legacy-menu-item'} aria-current={active ? 'page' : undefined} onClick={() => choose(item.id)}>
                    {item.title}
                  </button>);
              }
              if (sideMode) {
                  return (<button key={item.id} type="button" className={active ? 'legacy-menu-item is-active' : 'legacy-menu-item'} aria-current={active ? 'page' : undefined} onClick={() => choose(item.id)}>
                    {item.title}
                  </button>);
              }
              const open = openId === item.id;
              return (<div key={item.id} className={open ? 'legacy-menu-group is-open' : 'legacy-menu-group'} onMouseEnter={() => setOpenId(item.id)} onMouseLeave={() => setOpenId('')}>
                <button type="button" className={active ? 'legacy-menu-item is-active' : 'legacy-menu-item'} aria-haspopup="true" aria-expanded={open} onClick={() => setOpenId(item.id)}>
                  {item.title}
                </button>
                <SubMenu items={item.children} label={item.title} open={open} selectedPath={selectedPath} onChoose={choose}/>
              </div>);
          })}
        </nav>
        </>)}

        <div className="legacy-body">
          {sideModule ? (<aside className="legacy-side ax-side" aria-label={`${sideModule.title} menu`}>
            <SideMenu module={sideModule} selectedPath={selectedPath} openGroups={openSideGroups} search={sideSearch} onSearch={setSideSearch} onToggle={(id, open) => setOpenSideGroups((current) => ({ ...current, [id]: open }))} onChoose={choose}/>
          </aside>) : null}
          <main className="legacy-content">
            {sideMode && selectedId !== 'home' ? (<div className="ax-crumbs">
              <button type="button" onClick={() => choose('home')}>Home</button>
              {selectedPath.map((node, index) => (<span key={node.id}>
                <span className="ax-crumb-sep">/</span>
                {index < selectedPath.length - 1 && node.children
                  ? <button type="button" onClick={() => choose(node.id)}>{node.title}</button>
                  : <span className={index === selectedPath.length - 1 ? 'ax-crumb-current' : undefined}>{node.title}</span>}
              </span>))}
            </div>) : null}
            {session && session.passwordChangeRequired && !sideMode
              ? (<>
                  <p className="lg-password-required" role="alert">Your password was set automatically or reset. Please change it before you continue.</p>
                  <ChangePasswordPage forced/>
                </>)
              : selectedId === 'home'
              ? children
              : Screen
                ? <Screen key={selectedId}/>
                : sideModule && selectedId === sideModule.id
                  ? (<div className="legacy-side-welcome">
                      <h1 className="legacy-content-title">{sideModule.title}</h1>
                      <p>Choose an option from the {sideModule.title} menu on the left.</p>
                    </div>)
                  : <h1 className="legacy-content-title">{selectedTitle}</h1>}
          </main>
        </div>

        {sideMode ? (<footer className="ax-footer">
          <div className="ax-footer-bar">
            <img src={acmeLogo} alt=""/>
            <span>&copy; {new Date().getFullYear()} {brand.productName}. All rights reserved.</span>
            <span className="ax-footer-contact">
              <a href={brand.phoneHref}>{brand.phoneDisplay}</a>
              <span className="ax-footer-sep">|</span>
              <a href={brand.emailHref}>{brand.email}</a>
            </span>
          </div>
        </footer>) : (
        <footer className="legacy-footer">
          <span>&copy; {new Date().getFullYear()} {brand.productName}. All rights reserved.</span>
          <span className="legacy-footer-contact">
            <a href={brand.phoneHref}>{brand.phoneDisplay}</a>
            <span className="legacy-pipe">|</span>
            <a href={brand.emailHref}>{brand.email}</a>
          </span>
        </footer>
        )}
      </div>
    </div>);
}
