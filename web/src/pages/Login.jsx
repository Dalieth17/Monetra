import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Api, Session, enterDemo } from '../api/index.js';
import { Icon } from '../components/Icon.jsx';

const emailOk = (v) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);

function Aside() {
  return (
    <div className="auth-aside">
      <div className="auth-logo">
        <svg viewBox="0 0 108 108"><circle cx="54" cy="54" r="30" fill="#00A86B" /><path d="M38 66 L48 50 L56 58 L70 40" stroke="#fff" strokeWidth="5" strokeLinecap="round" strokeLinejoin="round" fill="none" /><path d="M70 40 L71 49 L62 41 Z" fill="#fff" /></svg>
        <span>Monetra</span>
      </div>
      <div className="pitch">
        <h2>Tus finanzas, con claridad total.</h2>
        <p>Registra ingresos y egresos, controla deudas y metas, y observa tu patrimonio en pesos y dólares — todo en un solo lugar.</p>
        <div className="feat"><Icon name="shield" size={20} /> Multimoneda COP y USD</div>
        <div className="feat"><Icon name="ai" size={20} /> Categorización asistida por IA</div>
        <div className="feat"><Icon name="chart" size={20} /> Mercado y tasa de cambio al día</div>
      </div>
    </div>
  );
}

export default function Login() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const canSubmit = emailOk(email) && password.length > 0 && !loading;

  const submit = async (e) => {
    e.preventDefault();
    if (!canSubmit) return;
    setLoading(true);
    setError(null);
    try {
      const auth = await Api.login(email.trim(), password);
      Session.save(auth);
      navigate('/dashboard');
    } catch (err) {
      setError(err.message);
      setLoading(false);
    }
  };

  const demo = () => {
    enterDemo();
    navigate('/dashboard');
  };

  return (
    <div className="auth">
      <Aside />
      <div className="auth-form-wrap">
        <form className="auth-card" onSubmit={submit}>
          <h1>Bienvenido de vuelta</h1>
          <p className="sub">Ingresa a tu cuenta de Monetra.</p>

          <div className={`field ${email && !emailOk(email) ? 'bad' : ''}`}>
            <label htmlFor="email">Correo electrónico</label>
            <input id="email" type="email" value={email} autoComplete="email"
              onChange={(e) => { setEmail(e.target.value); setError(null); }} />
            {email && !emailOk(email) && <span className="hint">Formato de correo inválido</span>}
          </div>
          <div className="field">
            <label htmlFor="pass">Contraseña</label>
            <input id="pass" type="password" value={password} autoComplete="current-password"
              onChange={(e) => { setPassword(e.target.value); setError(null); }} />
          </div>

          {error && <p className="inline-error">{error}</p>}

          <button className="btn full" type="submit" disabled={!canSubmit}>
            {loading ? 'Ingresando…' : 'Iniciar sesión'}
          </button>
          <div style={{ textAlign: 'center', marginTop: 10 }}>
            <Link className="btn-text" to="/registro">¿No tienes cuenta? Regístrate</Link>
          </div>

          <div className="auth-divider">o</div>
          <button className="btn ghost full" type="button" onClick={demo}>Entrar en modo demostración</button>
          <p className="form-note" style={{ textAlign: 'center' }}>Explora la app con datos de ejemplo, sin necesidad de servidor.</p>
        </form>
      </div>
    </div>
  );
}
