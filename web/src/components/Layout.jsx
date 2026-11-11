import { NavLink, useNavigate } from 'react-router-dom';
import { Icon } from './Icon.jsx';
import { Session } from '../api/index.js';

const NAV = [
  { to: '/dashboard', icon: 'grid', label: 'Panel' },
  { to: '/movimientos', icon: 'list', label: 'Movimientos' },
  { to: '/deudas', icon: 'card', label: 'Deudas' },
  { to: '/metas', icon: 'savings', label: 'Metas' },
  { to: '/suscripciones', icon: 'subs', label: 'Suscripciones' },
  { to: '/mercado', icon: 'chart', label: 'Mercado' }
];

function Brand() {
  return (
    <div className="brand">
      <svg viewBox="0 0 108 108"><rect width="108" height="108" rx="24" fill="#0B192C" /><circle cx="54" cy="54" r="30" fill="#00A86B" /><path d="M38 66 L48 50 L56 58 L70 40" stroke="#fff" strokeWidth="5" strokeLinecap="round" strokeLinejoin="round" fill="none" /><path d="M70 40 L71 49 L62 41 Z" fill="#fff" /></svg>
      <span>Monetra</span>
    </div>
  );
}

export default function Layout({ title, actions, children }) {
  const navigate = useNavigate();
  const name = Session.userName || 'Usuario';
  const initial = name.trim().charAt(0).toUpperCase() || 'M';

  const logout = () => {
    Session.clear();
    navigate('/login', { replace: true });
  };

  return (
    <div className="shell">
      <aside className="sidebar">
        <Brand />
        <nav className="nav">
          {NAV.map((n) => (
            <NavLink key={n.to} to={n.to} className={({ isActive }) => (isActive ? 'active' : '')}>
              <Icon name={n.icon} size={20} />
              {n.label}
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-foot">
          <div className="sidebar-user">
            <div className="avatar">{initial}</div>
            <div className="who">
              <b>{name}</b>
              <small>{Session.isDemo ? 'Modo demostración' : Session.userEmail}</small>
            </div>
          </div>
          <button className="logout-btn" onClick={logout}>Cerrar sesión</button>
        </div>
      </aside>

      <div className="main">
        <div className="mobile-top">
          <Brand />
          <div className="spacer" />
          {actions}
          <button className="icon-btn" onClick={logout} aria-label="Cerrar sesión"><Icon name="logout" size={20} /></button>
        </div>

        <header className="topbar">
          <h1>{title}</h1>
          {actions}
        </header>

        <main className="content">{children}</main>
      </div>

      <nav className="bottom-nav">
        {NAV.map((n) => (
          <NavLink key={n.to} to={n.to} className={({ isActive }) => (isActive ? 'active' : '')}>
            <Icon name={n.icon} size={22} />
            {n.label}
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
