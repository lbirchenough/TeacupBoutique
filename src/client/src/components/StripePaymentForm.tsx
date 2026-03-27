import { useState } from 'react'
import { loadStripe } from '@stripe/stripe-js'
import { Elements, PaymentElement, useStripe, useElements } from '@stripe/react-stripe-js'
import { Turnstile } from '@marsidev/react-turnstile'
import { paymentsApi } from '../lib/paymentsApi'
import { useQuery } from '@tanstack/react-query'

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY)

// Inner form — must be inside <Elements> to use useStripe/useElements
function PaymentForm({ onSuccess }: { onSuccess: () => void }) {
    const stripe = useStripe()
    const elements = useElements()
    const [error, setError] = useState<string | null>(null)
    const [processing, setProcessing] = useState(false)

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        if (!stripe || !elements) return

        setProcessing(true)
        setError(null)

        const { error: stripeError } = await stripe.confirmPayment({
            elements,
            redirect: 'if_required', // stay on page for cards, only redirect for redirect-based methods
        })

        if (stripeError) {
            setError(stripeError.message ?? 'Payment failed. Please try again.')
            setProcessing(false)
        } else {
            // Payment confirmed — polling on the order page will pick up the webhook result
            onSuccess()
        }
    }

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <PaymentElement />
            {error && <p className="text-sm text-red-600">{error}</p>}
            <button
                type="submit"
                disabled={!stripe || processing}
                className="w-full bg-brown hover:bg-brown-mid disabled:opacity-50 text-cream font-semibold px-8 py-2.5 text-xs tracking-widest uppercase transition-colors"
            >
                {processing ? 'Processing…' : 'Pay Now'}
            </button>
        </form>
    )
}

// Outer component — fetches the clientSecret then mounts Elements
interface StripePaymentFormProps {
    orderId: string
    amount: number
    onSuccess: () => void
}

export function StripePaymentForm({ orderId, amount, onSuccess }: StripePaymentFormProps) {
    const [turnstileToken, setTurnstileToken] = useState<string | null>(null)
    const [turnstileFailed, setTurnstileFailed] = useState(false)

    const { data, isPending, isError } = useQuery({
        queryKey: ['payment-intent', orderId],
        queryFn: () => paymentsApi.createIntent(orderId, amount, turnstileToken!),
        enabled: !!turnstileToken,
        staleTime: Infinity, // don't re-create the intent on refetch
    })

    if (!turnstileToken && !turnstileFailed) return (
        <>
            <p className="text-sm text-brown-light">Preparing secure payment…</p>
            <Turnstile
                siteKey={import.meta.env.VITE_TURNSTILE_SITE_KEY_INVISIBLE}
                options={{ appearance: 'invisible' }}
                onSuccess={(token) => setTurnstileToken(token)}
                onError={() => setTurnstileFailed(true)}
            />
        </>
    )

    if (turnstileFailed && !turnstileToken) return (
        <Turnstile
            siteKey={import.meta.env.VITE_TURNSTILE_SITE_KEY_MANAGED}
            onSuccess={(token) => { setTurnstileToken(token); setTurnstileFailed(false) }}
        />
    )

    if (isPending) return <p className="text-sm text-brown-light">Preparing payment…</p>
    if (isError) return <p className="text-sm text-red-600">Could not initialise payment. Please refresh and try again.</p>

    return (
        <Elements stripe={stripePromise} options={{ clientSecret: data.clientSecret }}>
            <PaymentForm onSuccess={onSuccess} />
        </Elements>
    )
}
