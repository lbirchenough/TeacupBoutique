import { TermsContent } from './TermsContent'

interface TermsModalProps {
    onClose: () => void
}

export function TermsModal({ onClose }: TermsModalProps) {
    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-black/40" onClick={onClose} />
            <div className="relative bg-cream w-full max-w-2xl max-h-[85vh] flex flex-col shadow-xl">
                <div className="flex items-center justify-between px-8 py-5 border-b border-gold/20 shrink-0">
                    <h2 className="font-serif text-2xl text-brown">Terms & Conditions</h2>
                    <button onClick={onClose} className="text-brown-light hover:text-brown transition-colors text-xl leading-none">✕</button>
                </div>
                <div className="overflow-y-auto px-8 py-6">
                    <TermsContent />
                </div>
                <div className="px-8 py-5 border-t border-gold/20 shrink-0">
                    <button
                        onClick={onClose}
                        className="w-full bg-brown hover:bg-brown-mid text-cream py-3 text-xs font-semibold tracking-[0.15em] uppercase transition-colors"
                    >
                        Close
                    </button>
                </div>
            </div>
        </div>
    )
}
