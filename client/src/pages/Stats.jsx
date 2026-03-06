import { useState, useEffect } from 'react';
import { getStats } from '../api';

function formatDate(d) {
  return new Date(d).toLocaleDateString();
}

export default function Stats() {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    getStats()
      .then(setStats)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <p>Загрузка...</p>;
  if (error) return <p className="error">Ошибка: {error}</p>;
  if (!stats) return null;

  return (
    <section>
      <h2>Статистика платежей</h2>
      <div className="stats-summary">
        <p><strong>Общая сумма платежей:</strong> {stats.totalAmount}</p>
        <p><strong>Количество платежей:</strong> {stats.totalCount}</p>
      </div>
      <h3>Агрегация по дням</h3>
      <table>
        <thead>
          <tr>
            <th>Дата</th>
            <th>Количество</th>
            <th>Сумма за день</th>
          </tr>
        </thead>
        <tbody>
          {stats.byDay?.length === 0 ? (
            <tr><td colSpan={3}>Нет данных</td></tr>
          ) : (
            (stats.byDay || []).map((row) => (
              <tr key={row.date}>
                <td>{formatDate(row.date)}</td>
                <td>{row.count}</td>
                <td>{row.sum}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </section>
  );
}
