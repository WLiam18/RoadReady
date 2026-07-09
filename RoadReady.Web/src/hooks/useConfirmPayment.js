import { useState } from 'react';

// Lightweight hook: triggered after the Razorpay hosted payment link completes.

export default function useConfirmPayment() {
  const [busy, setBusy] = useState(false);

  const confirmPayment = async (_bookingId) => {
    setBusy(true);
    try {
      await new Promise((r) => setTimeout(r, 200));
      return true;
    } finally {
      setBusy(false);
    }
  };

  return { busy, confirmPayment };
}
