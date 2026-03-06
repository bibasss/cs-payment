import { useState, useEffect } from 'react';
import { getPayments } from '../api';

function formatDate(d) {
  return new Date(d).toLocaleString();
}

export default function History() {
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [sort, setSort] = useState('desc');

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getPayments({ sort: sort === 'asc' ? 'asc' : undefined });
      setPayments(Array.isArray(data) ? data : []);
    } catch (e) {
      setError(e.message);
      setPayments([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [sort]);

  if (loading) return <p>Загрузка...</p>;
  if (error) return <p className="error">Ошибка: {error}</p>;

  return (
    <section>
      <h2>История платежей</h2>
      <p>
        Сортировка по дате:
        <button type="button" onClick={() => setSort('desc')} className={sort === 'desc' ? 'active' : ''}>Сначала новые</button>
        <button type="button" onClick={() => setSort('asc')} className={sort === 'asc' ? 'active' : ''}>Сначала старые</button>
      </p>
      <table>
        <thead>
          <tr>
            <th>Дата создания</th>
            <th>Аккаунт</th>
            <th>Email</th>
            <th>Сумма</th>
            <th>Валюта</th>
            <th>Статус</th>
            <th>Комментарий</th>
          </tr>
        </thead>
        <tbody>
          {payments.length === 0 ? (
            <tr><td colSpan={7}>Нет платежей</td></tr>
          ) : (
            payments.map((p) => (
              <tr key={p.id}>
                <td>{formatDate(p.createdAt)}</td>
                <td>{p.account}</td>
                <td>{p.email}</td>
                <td>{p.amount}</td>
                <td>{p.currency}</td>
                <td>{p.status}</td>
                <td>{p.comment ?? '—'}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </section>
  );
}
