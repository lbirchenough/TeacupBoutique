export function TermsContent() {
    return (
        <div className="space-y-7 text-sm text-brown-mid leading-relaxed">

            <section>
                <h3 className="font-serif text-lg text-brown mb-2">Bookings & Reservations</h3>
                <p>All bookings are subject to availability. Your reservation is confirmed once payment has been received in full. By placing an order you agree to provide accurate contact details and to be available for collection and return on the agreed dates.</p>
            </section>

            <section>
                <h3 className="font-serif text-lg text-brown mb-2">Security Deposit</h3>
                <p>A refundable security deposit is charged at the time of payment and held against the safe return of all hired items. The deposit amount is displayed at checkout and on your order confirmation.</p>
                <p className="mt-2">The deposit will be refunded in full once all items are returned in good condition. Teacup Boutique reserves the right to retain part or all of the deposit to cover the cost of damage or loss.</p>
            </section>

            <section>
                <h3 className="font-serif text-lg text-brown mb-2">Refunds & Cancellations</h3>
                <p>You may cancel your order at any time before the reservation date. The following refund policy applies:</p>
                <ul className="mt-2 space-y-1.5 list-disc list-inside text-brown-mid">
                    <li><span className="font-medium text-brown">More than 30 days before reservation:</span> Full refund of the hire amount and security deposit.</li>
                    <li><span className="font-medium text-brown">30 days or less before reservation:</span> Security deposit refunded only. The hire amount is non-refundable as it represents lost revenue from the late cancellation.</li>
                </ul>
                <p className="mt-2">Refunds are processed to the original payment method and may take 5–10 business days to appear depending on your bank.</p>
            </section>

            <section>
                <h3 className="font-serif text-lg text-brown mb-2">Care of Equipment</h3>
                <p>All hired items must be treated with care during your event. You do not need to clean the items. We handle all cleaning in-house using our own process to ensure the safety of delicate pieces.</p>
                <p className="mt-2">Before returning, please:</p>
                <ul className="mt-1.5 space-y-1 list-disc list-inside text-brown-mid">
                    <li>Repack all items in the packaging provided</li>
                    <li>Remove any leftover food, liquid, or debris from items</li>
                    <li>Ensure nothing is left loose or unprotected during transit</li>
                </ul>
                <p className="mt-2">Teacup Boutique reserves the right to charge for replacement of any items that are lost, broken, or returned in an unacceptable condition.</p>
            </section>

            <section>
                <h3 className="font-serif text-lg text-brown mb-2">Collection & Return</h3>
                <p>Collection and return times will be confirmed with you prior to your reservation date. Late returns may incur additional hire charges. Please ensure someone is available to hand back items on the agreed return date.</p>
            </section>

            <section>
                <h3 className="font-serif text-lg text-brown mb-2">General</h3>
                <p>Teacup Boutique is not liable for any loss, damage, or injury arising from the use of hired items. These terms are governed by the laws of Australia. By placing an order you confirm that you have read, understood, and agree to these terms and conditions.</p>
            </section>

        </div>
    )
}
