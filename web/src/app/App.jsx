import { Navigate, Route, Routes } from 'react-router-dom';
import { ToastProvider } from '../components/Toast.jsx';
import { Session } from '../api/index.js';
import Login from '../pages/Login.jsx';
import Register from '../pages/Register.jsx';
import Dashboard from '../pages/Dashboard.jsx';
import Transactions from '../pages/Transactions.jsx';
import Accounts from '../pages/Accounts.jsx';
import Debts from '../pages/Debts.jsx';
import Savings from '../pages/Savings.jsx';
import Subscriptions from '../pages/Subscriptions.jsx';
import Market from '../pages/Market.jsx';

function RequireAuth({ children }) {
  return Session.isLoggedIn() ? children : <Navigate to="/login" replace />;
}

export default function App() {
  return (
    <ToastProvider>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/registro" element={<Register />} />
        <Route path="/dashboard" element={<RequireAuth><Dashboard /></RequireAuth>} />
        <Route path="/movimientos" element={<RequireAuth><Transactions /></RequireAuth>} />
        <Route path="/cuentas" element={<RequireAuth><Accounts /></RequireAuth>} />
        <Route path="/deudas" element={<RequireAuth><Debts /></RequireAuth>} />
        <Route path="/metas" element={<RequireAuth><Savings /></RequireAuth>} />
        <Route path="/suscripciones" element={<RequireAuth><Subscriptions /></RequireAuth>} />
        <Route path="/mercado" element={<RequireAuth><Market /></RequireAuth>} />
        <Route path="*" element={<Navigate to={Session.isLoggedIn() ? '/dashboard' : '/login'} replace />} />
      </Routes>
    </ToastProvider>
  );
}
