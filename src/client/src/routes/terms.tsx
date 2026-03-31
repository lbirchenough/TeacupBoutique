import { createFileRoute } from '@tanstack/react-router'
import { TermsContent } from '../components/TermsContent'

export const Route = createFileRoute('/terms')({
    component: TermsPage,
})

function TermsPage() {
    return (
        <div>
            <div className="bg-cream-dark border-b border-gold/20 py-16 text-center">
                <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-4">Legal</p>
                <h1 className="font-serif text-4xl text-brown">Terms & Conditions</h1>
            </div>

            <div className="max-w-2xl mx-auto px-6 py-14">
                <TermsContent />
            </div>
        </div>
    )
}
