// Estado de sesión persistido en localStorage.
const KEYS = ['monetra_token', 'monetra_name', 'monetra_email', 'monetra_demo'];

export const Session = {
  get token() { return localStorage.getItem('monetra_token'); },
  get userName() { return localStorage.getItem('monetra_name') || ''; },
  get userEmail() { return localStorage.getItem('monetra_email') || ''; },
  get isDemo() { return localStorage.getItem('monetra_demo') === '1'; },
  isLoggedIn() { return !!this.token; },

  save(auth) {
    localStorage.setItem('monetra_token', auth.token);
    localStorage.setItem('monetra_name', auth.name);
    localStorage.setItem('monetra_email', auth.email);
  },

  enterDemo(auth) {
    localStorage.setItem('monetra_demo', '1');
    this.save(auth);
  },

  clear() {
    KEYS.forEach((k) => localStorage.removeItem(k));
  }
};
