import { useState } from 'react';
import { createPayment } from '../api';

const CURRENCIES = ['RUB', 'USD', 'EUR'];

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const phoneRegex = /^\+?[\d\s\-()]{10,20}$/;

export default function CreatePayment({ onSuccess, onError }) {
  const [form, setForm] = useState({
    walletNumber: '',
    account: '',
    email: '',
    phone: '',
    amount: '',
    currency: 'RUB',
    comment: '',
  });
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  const validate = () => {
    const e = {};
    if (!form.walletNumber.trim()) e.walletNumber = 'Обязательное поле';
    if (!form.account.trim()) e.account = 'Обязательное поле';
    if (!form.email.trim()) e.email = 'Обязательное поле';
    else if (!emailRegex.test(form.email)) e.email = 'Неверный формат email';
    if (form.phone.trim() && !phoneRegex.test(form.phone)) e.phone = 'Неверный формат телефона';
    const amount = parseFloat(form.amount);
    if (form.amount === '' || isNaN(amount) || amount <= 0) e.amount = 'Сумма должна быть положительной';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSubmit = async (ev) => {
    ev.preventDefault();
    if (!validate()) return;
    setLoading(true);
    setErrors({});
    try {
      await createPayment({
        walletNumber: form.walletNumber.trim(),
        account: form.account.trim(),
        email: form.email.trim(),
        phone: form.phone.trim() || undefined,
        amount: parseFloat(form.amount),
        currency: form.currency,
        comment: form.comment.trim() || undefined,
      });
      setForm({ walletNumber: '', account: '', email: '', phone: '', amount: '', currency: 'RUB', comment: '' });
      onSuccess?.();
    } catch (err) {
      onError?.(err.message);
      setErrors({ submit: err.message });
    } finally {
      setLoading(false);
    }
  };

  return (
    <section>
      <h2>Создание платежа</h2>
      <form onSubmit={handleSubmit}>
        <div>
          <label>Номер кошелька *</label>
          <input value={form.walletNumber} onChange={(e) => setForm((f) => ({ ...f, walletNumber: e.target.value }))} />
          {errors.walletNumber && <span className="error">{errors.walletNumber}</span>}
        </div>
        <div>
          <label>Аккаунт / UserId *</label>
          <input value={form.account} onChange={(e) => setForm((f) => ({ ...f, account: e.target.value }))} />
          {errors.account && <span className="error">{errors.account}</span>}
        </div>
        <div>
          <label>Email *</label>
          <input type="email" value={form.email} onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))} />
          {errors.email && <span className="error">{errors.email}</span>}
        </div>
        <div>
          <label>Телефон</label>
          <input value={form.phone} onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))} placeholder="+7 ..." />
          {errors.phone && <span className="error">{errors.phone}</span>}
        </div>
        <div>
          <label>Сумма *</label>
          <input type="number" step="0.01" min="0" value={form.amount} onChange={(e) => setForm((f) => ({ ...f, amount: e.target.value }))} />
          {errors.amount && <span className="error">{errors.amount}</span>}
        </div>
        <div>
          <label>Валюта</label>
          <select value={form.currency} onChange={(e) => setForm((f) => ({ ...f, currency: e.target.value }))}>
            {CURRENCIES.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </select>
        </div>
        <div>
          <label>Комментарий</label>
          <textarea value={form.comment} onChange={(e) => setForm((f) => ({ ...f, comment: e.target.value }))} rows={2} />
        </div>
        {errors.submit && <div className="error">{errors.submit}</div>}
        <button type="submit" disabled={loading}>{loading ? 'Отправка...' : 'Создать платёж'}</button>
      </form>
    </section>
  );
}
