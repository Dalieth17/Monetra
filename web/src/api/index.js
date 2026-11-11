import { Session } from './session.js';
import { RealApi } from './real.js';
import { DemoApi } from './demo.js';

// Enruta cada llamada al backend real o al modo demo según la sesión.
// Los componentes usan Api.xxx sin saber en qué modo están.
export const Api = new Proxy({}, {
  get(_, prop) {
    if (Session.isDemo && typeof DemoApi[prop] === 'function') {
      return DemoApi[prop].bind(DemoApi);
    }
    return RealApi[prop];
  }
});

export function enterDemo() {
  const auth = DemoApi.enter();
  Session.enterDemo(auth);
}

export { Session };
