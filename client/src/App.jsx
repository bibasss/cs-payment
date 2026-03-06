import { useState } from 'react';
import CreatePayment from './pages/CreatePayment';
import History from './pages/History';
import Stats from './pages/Stats';
import './App.css';

const TABS = [
  { id: 'create', label: 'Создание платежа' },
  { id: 'history', label: 'История платежей' },
  { id: 'stats', label: 'Статистика' },
];

export default function App() {
  const [tab, setTab] = useState('create');
  const [message, setMessage] = useState(null);

  return (
    <div className="app">
      <h1>Платежная форма</h1>
      <nav>
        {TABS.map((t) => (
          <button key={t.id} type="button" className={tab === t.id ? 'active' : ''} onClick={() => { setTab(t.id); setMessage(null); }}>
            {t.label}
          </button>
        ))}
      </nav>
      {message && <div className="message">{message}</div>}
      {tab === 'create' && (
        <CreatePayment
          onSuccess={() => setMessage('Платёж успешно создан.')}
          onError={(msg) => setMessage(msg)}
        />
      )}
      {tab === 'history' && <History />}
      {tab === 'stats' && <Stats />}
    </div>
  );
}
