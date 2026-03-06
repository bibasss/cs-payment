const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5087';
const API_KEY = import.meta.env.VITE_API_KEY || 'PaymentFormApiKey-ChangeInProduction';

const headers = () => ({
  'Content-Type': 'application/json',
  'X-Api-Key': API_KEY,
});

export async function createPayment(data) {
  const res = await fetch(`${API_URL}/api/payments`, {
    method: 'POST',
    headers: headers(),
    body: JSON.stringify(data),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.error || res.statusText);
  }
  return res.json();
}

export async function getPayments(params = {}) {
  const q = new URLSearchParams(params).toString();
  const url = `${API_URL}/api/payments${q ? `?${q}` : ''}`;
  const res = await fetch(url, { headers: headers() });
  if (!res.ok) throw new Error(res.statusText);
  return res.json();
}

export async function getStats() {
  const res = await fetch(`${API_URL}/api/payments/stats`, { headers: headers() });
  if (!res.ok) throw new Error(res.statusText);
  return res.json();
}
