import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Api, Session } from '../api/index.js';
import { Icon } from '../components/Icon.jsx';

const emailOk = (v) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);

export default function Register() {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const passOk = password.length >= 6;
  const canSubmit = name.trim() && emailOk(email) && passOk && !loading;

  const submit = async (e) => {
    e.preventDefault();
    if (!canSubmit) return;
    setLoading(true);
    setError(null);
    try {
      const auth = await Api.register(name.trim(), email.trim(), password);
      Session.save(auth);
      navigate('/dashboard');
    } catch (err) {
      setError(err.message);
      setLoading(false);
    }
  };

  return (
    <div className="auth">
      <div className="auth-aside">
        <div className="auth-logo">
          <svg viewBox="0 0 108 108"><circle cx="54" cy="54" r="30" fill="#00A86B" /><path d="M38 66 L48 50 L56 58 L70 40" stroke="#fff" strokeWidth="5" strokeLinecap="round" strokeLinejoin="round" fill="none" /><path d="M70 40 L71 49 L62 41 Z" fill="#fff" /></svg>
          <span>Monetra</span>
        </div>
        <div className="pitch">
          <h2>Empieza a tomar el control hoy.</h2>
          <p>Al crear tu cuenta preparamos automáticamente tus billeteras en pesos y dólares para que registres tu primer movimiento en segundos.</p>
          <div className="feat"><Icon name="cash" size={20} /> Cuentas COP y USD listas</div>
        </div>
      </div>
      <div className="auth-form-wrap">
        <form className="auth-card" onSubmit={submit}>
          <h1>Crear cuenta</h1>
          <p className="sub">Se crearán automáticamente tus cuentas en COP y USD.</p>

          <div className="field">
            <label htmlFor="name">Nombre completo</label>
            <input id="name" type="text" value={name} autoComplete="name"
              onChange={(e) => { setName(e.target.value); setError(null); }} />
          </div>
          <div className={`field ${email && !emailOk(email) ? 'bad' : ''}`}>
            <label htmlFor="email">Correo electrónico</label>
            <input id="email" type="email" value={email} autoComplete="email"
              onChange={(e) => { setEmail(e.target.value); setError(null); }} />
            {email && !emailOk(email) && <span className="hint">Formato de correo inválido</span>}
          </div>
          <div className={`field ${password && !passOk ? 'bad' : ''}`}>
            <label htmlFor="pass">Contraseña</label>
            <input id="pass" type="password" value={password} autoComplete="new-password"
              onChange={(e) => { setPassword(e.target.value); setError(null); }} />
            {password && !passOk && <span className="hint">La contraseña debe tener al menos 6 caracteres</span>}
          </div>

          {error && <p className="inline-error">{error}</p>}

          <button className="btn full" type="submit" disabled={!canSubmit}>
            {loading ? 'Creando cuenta…' : 'Crear cuenta'}
          </button>
          <div style={{ textAlign: 'center', marginTop: 10 }}>
            <Link className="btn-text" to="/login">Ya tengo una cuenta</Link>
          </div>
        </form>
      </div>
    </div>
  );
}
