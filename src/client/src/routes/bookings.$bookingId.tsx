import { createFileRoute, Link } from '@tanstack/react-router'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { bookingsApi, type BookingItemAssessmentDto, type SetItemAssessmentDto } from '../lib/bookingsApi'
import { ordersApi } from '../lib/ordersApi'
import type { BookingDetail, BookingStatus, OrderDetail, ReturnAssessmentDetail, SetItemDetail } from '../lib/types'

export const Route = createFileRoute('/bookings/$bookingId')({
    component: BookingDetailPage,
})

const statusConfig: Record<BookingStatus, { label: string; bg: string; text: string }> = {
    Reserved:   { label: 'Reserved',    bg: 'bg-amber-50',  text: 'text-amber-800' },
    Confirmed:  { label: 'Confirmed',   bg: 'bg-blue-50',   text: 'text-blue-800' },
    CheckedOut: { label: 'Checked Out', bg: 'bg-purple-50', text: 'text-purple-800' },
    Returned:   { label: 'Returned',    bg: 'bg-green-50',  text: 'text-green-800' },
    Completed:  { label: 'Completed',   bg: 'bg-cream-dark',text: 'text-brown-mid' },
    Cancelled:  { label: 'Cancelled',   bg: 'bg-red-50',    text: 'text-red-800' },
}

// Assessment state per booking item: map of bookingItemId → map of setItemId → {good, damaged, missing}
type ComponentAssessment = { good: number; damaged: number; missing: number }
type ItemAssessmentState = Record<string, Record<string, ComponentAssessment>>

function calcDeduction(assessmentState: ItemAssessmentState, booking: BookingDetail): number {
    let deduction = 0
    for (const [, components] of Object.entries(assessmentState)) {
        for (const [setItemId, counts] of Object.entries(components)) {
            const setItems = Object.values(booking.setItemsByProduct ?? {}).flat()
            const setItem = setItems.find(si => si.id === setItemId)
            if (setItem) {
                deduction += (counts.damaged + counts.missing) * setItem.depositValuePerUnit
            }
        }
    }
    return deduction
}

function calcDepositTotal(booking: BookingDetail): number {
    if (!booking.setItemsByProduct) return 0
    return booking.bookingItems.reduce((total, bi) => {
        const items = booking.setItemsByProduct![bi.productId] ?? []
        return total + items.reduce((sum, si) => sum + si.quantity * si.depositValuePerUnit, 0)
    }, 0)
}

function calcCompletionRefund(booking: BookingDetail): { depositTotal: number; deduction: number; refund: number } {
    const depositTotal = calcDepositTotal(booking)
    const allSetItems = Object.values(booking.setItemsByProduct ?? {}).flat()
    let deduction = 0
    for (const bi of booking.bookingItems) {
        for (const ra of bi.returnAssessments ?? []) {
            const si = allSetItems.find(s => s.id === ra.setItemId)
            if (si) {
                const stillMissing = Math.max(0, ra.quantityMissing - ra.quantityCustomerReturned)
                deduction += (ra.quantityDamaged + stillMissing) * si.depositValuePerUnit
            }
        }
    }
    return { depositTotal, deduction, refund: Math.max(0, depositTotal - deduction) }
}

function BookingDetailPage() {
    const { bookingId } = Route.useParams()
    const queryClient = useQueryClient()

    const [showReturnForm, setShowReturnForm] = useState(false)
    const [assessmentState, setAssessmentState] = useState<ItemAssessmentState>({})
    const [returnNotes, setReturnNotes] = useState<Record<string, string>>({})
    const [showCancelConfirm, setShowCancelConfirm] = useState(false)
    const [completionNotes, setCompletionNotes] = useState('')
    const [uploadedPhotoUrls, setUploadedPhotoUrls] = useState<string[]>([])
    const [uploadingPhoto, setUploadingPhoto] = useState(false)
    const [uploadError, setUploadError] = useState<string | null>(null)

    const { data: booking, isPending, isError } = useQuery<BookingDetail>({
        queryKey: ['booking', bookingId],
        queryFn: () => bookingsApi.getBooking(bookingId),
    })

    const { data: order } = useQuery<OrderDetail>({
        queryKey: ['order-admin', booking?.orderId],
        queryFn: () => ordersApi.getOrderByIdAdmin(booking!.orderId),
        enabled: !!booking?.orderId,
    })

    const invalidate = () => {
        queryClient.invalidateQueries({ queryKey: ['booking', bookingId] })
        queryClient.invalidateQueries({ queryKey: ['order-admin', booking?.orderId] })
        queryClient.invalidateQueries({ queryKey: ['bookings'] })
    }

    const checkOutMutation = useMutation({
        mutationFn: () => bookingsApi.checkOut(bookingId),
        onSuccess: invalidate,
    })

    const returnMutation = useMutation({
        mutationFn: () => {
            const items: BookingItemAssessmentDto[] = booking!.bookingItems.map(bi => ({
                bookingItemId: bi.id,
                components: (booking!.setItemsByProduct?.[bi.productId] ?? []).map(si => {
                    const counts = assessmentState[bi.id]?.[si.id] ?? { good: si.quantity, damaged: 0, missing: 0 }
                    return {
                        setItemId: si.id,
                        quantityGood: counts.good,
                        quantityDamaged: counts.damaged,
                        quantityMissing: counts.missing,
                    } satisfies SetItemAssessmentDto
                }),
                returnNotes: returnNotes[bi.id] || null,
            }))
            return bookingsApi.markReturned(bookingId, items)
        },
        onSuccess: () => {
            setShowReturnForm(false)
            setAssessmentState({})
            setReturnNotes({})
            invalidate()
            queryClient.invalidateQueries({ queryKey: ['maintenance-queue'] })
        },
    })

    const markMissingReturnedMutation = useMutation({
        mutationFn: (assessmentId: string) => bookingsApi.markMissingItemReturned(bookingId, assessmentId),
        onSuccess: () => {
            invalidate()
            queryClient.invalidateQueries({ queryKey: ['spare-stock'] })
        },
    })

    const completeMutation = useMutation({
        mutationFn: () => bookingsApi.complete(bookingId, {
            completionNotes: completionNotes.trim() || undefined,
            returnPhotoUrls: uploadedPhotoUrls.length > 0 ? uploadedPhotoUrls : undefined,
        }),
        onSuccess: () => {
            setCompletionNotes('')
            setUploadedPhotoUrls([])
            invalidate()
        },
    })

    const handlePhotoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = Array.from(e.target.files ?? [])
        if (files.length === 0) return
        setUploadingPhoto(true)
        setUploadError(null)
        try {
            const urls = await Promise.all(files.map(f => bookingsApi.uploadPhoto(bookingId, f)))
            setUploadedPhotoUrls(prev => [...prev, ...urls])
        } catch (err) {
            setUploadError(err instanceof Error ? err.message : 'Upload failed')
        } finally {
            setUploadingPhoto(false)
            e.target.value = ''
        }
    }

    const cancelMutation = useMutation({
        mutationFn: () => bookingsApi.cancelBooking(bookingId),
        onSuccess: () => {
            setShowCancelConfirm(false)
            invalidate()
        },
    })

    if (isPending) return (
        <div className="max-w-3xl mx-auto px-6 py-20 text-center">
            <p className="font-serif text-lg text-brown-light">Loading booking…</p>
        </div>
    )

    if (isError || !booking) return (
        <div className="max-w-3xl mx-auto px-6 py-20 text-center">
            <p className="text-red-600">Booking not found.</p>
        </div>
    )

    const config = statusConfig[booking.status]
    const depositTotal = calcDepositTotal(booking)
    const currentDeduction = calcDeduction(assessmentState, booking)
    const hasMissingInForm = Object.values(assessmentState).some(components =>
        Object.values(components).some(c => c.missing > 0)
    )

    // Validate totals are correct for all items
    const allTotalsValid = !showReturnForm || booking.bookingItems.every(bi => {
        const setItems = booking.setItemsByProduct?.[bi.productId] ?? []
        return setItems.every(si => {
            const counts = assessmentState[bi.id]?.[si.id] ?? { good: si.quantity, damaged: 0, missing: 0 }
            return counts.good + counts.damaged + counts.missing === si.quantity
        })
    })

    const initReturnForm = () => {
        const initial: ItemAssessmentState = {}
        booking.bookingItems.forEach(bi => {
            initial[bi.id] = {}
            const setItems = booking.setItemsByProduct?.[bi.productId] ?? []
            setItems.forEach(si => {
                initial[bi.id][si.id] = { good: si.quantity, damaged: 0, missing: 0 }
            })
        })
        setAssessmentState(initial)
        setReturnNotes({})
        setShowReturnForm(true)
    }

    const updateCount = (bookingItemId: string, setItemId: string, field: keyof ComponentAssessment, value: number) => {
        setAssessmentState(prev => {
            const biState = prev[bookingItemId] ?? {}
            const si = booking.setItemsByProduct?.[
                booking.bookingItems.find(bi => bi.id === bookingItemId)?.productId ?? ''
            ]?.find(s => s.id === setItemId)
            if (!si) return prev
            const current = biState[setItemId] ?? { good: si.quantity, damaged: 0, missing: 0 }
            const updated = { ...current, [field]: Math.max(0, value) }
            // Auto-adjust good to keep total = quantity
            const total = updated.damaged + updated.missing
            updated.good = Math.max(0, si.quantity - total)
            return { ...prev, [bookingItemId]: { ...biState, [setItemId]: updated } }
        })
    }

    // Pending missing items: show whenever booking is Returned and there are missing assessments
    const allMissingAssessments: Array<ReturnAssessmentDetail & { bookingItemId: string }> =
        booking.bookingItems.flatMap(bi =>
            (bi.returnAssessments ?? [])
                .filter(ra => ra.quantityMissing > 0)
                .map(ra => ({ ...ra, bookingItemId: bi.id }))
        )

    const hasPendingMissing = booking.status === 'Returned' && allMissingAssessments.length > 0
    const completion = booking.status === 'Returned' ? calcCompletionRefund(booking) : null

    return (
        <div>
            <div className="bg-cream-dark border-b border-gold/20 py-16 text-center">
                <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-4">Booking</p>
                <h1 className="font-serif text-3xl text-brown">{booking.id.slice(0, 8).toUpperCase()}</h1>
                <p className="text-brown-light text-sm mt-2">
                    Created {new Date(booking.createdAt).toLocaleDateString('en-AU', {
                        day: 'numeric', month: 'long', year: 'numeric'
                    })}
                </p>
            </div>

            <div className="max-w-3xl mx-auto px-6 py-12 space-y-6">
                <Link to="/bookings" className="text-xs text-brown-light hover:text-brown transition-colors tracking-widest uppercase">
                    ← All Bookings
                </Link>

                {/* Status banner + action buttons */}
                <div className={`border border-gold/20 px-6 py-5 ${config.bg}`}>
                    <div className="flex items-center justify-between">
                        <p className={`font-serif text-lg ${config.text}`}>{config.label}</p>
                        <div className="flex gap-3">
                            {booking.status === 'Confirmed' && (
                                <ActionButton
                                    label="Mark as Checked Out"
                                    onClick={() => checkOutMutation.mutate()}
                                    isPending={checkOutMutation.isPending}
                                />
                            )}
                            {booking.status === 'CheckedOut' && !showReturnForm && (
                                <ActionButton
                                    label="Mark as Returned"
                                    onClick={initReturnForm}
                                    isPending={false}
                                />
                            )}
                            {(booking.status === 'Reserved' || booking.status === 'Confirmed') && !showCancelConfirm && (
                                <button
                                    onClick={() => setShowCancelConfirm(true)}
                                    className="text-xs tracking-widest uppercase border border-red-300 text-red-600 px-4 py-2 hover:bg-red-50 transition-colors"
                                >
                                    Cancel Booking
                                </button>
                            )}
                        </div>
                    </div>
                    {showCancelConfirm && (
                        <div className="mt-4 pt-4 border-t border-gold/20 space-y-3">
                            <p className="text-sm text-brown font-serif">Cancel this booking and the associated order?</p>
                            <p className="text-xs text-brown-light">The customer will receive a cancellation email.</p>
                            <div className="flex gap-3">
                                <button
                                    onClick={() => cancelMutation.mutate()}
                                    disabled={cancelMutation.isPending}
                                    className="text-xs bg-red-600 hover:bg-red-700 text-white px-5 py-2 transition-colors disabled:opacity-50"
                                >
                                    {cancelMutation.isPending ? 'Cancelling…' : 'Yes, Cancel Booking'}
                                </button>
                                <button
                                    onClick={() => setShowCancelConfirm(false)}
                                    className="text-xs text-brown-light hover:text-brown transition-colors px-3"
                                >
                                    Keep Booking
                                </button>
                            </div>
                            {cancelMutation.isError && (
                                <p className="text-xs text-red-600">{cancelMutation.error.message}</p>
                            )}
                        </div>
                    )}
                </div>

                {/* Pending missing items panel */}
                {hasPendingMissing && (
                    <div className="bg-amber-50 border border-amber-200 p-6 space-y-4">
                        <h2 className="font-serif text-lg text-amber-900">Pending Missing Items</h2>
                        <p className="text-sm text-amber-800">
                            The following items were reported missing at return. Mark each as returned when the customer brings them back — spare stock will be updated and the deposit calculation will adjust.
                        </p>
                        <div className="space-y-2">
                            {allMissingAssessments.map(ra => {
                                const remaining = ra.quantityMissing - ra.quantityCustomerReturned
                                const allReturned = remaining <= 0
                                return (
                                    <div key={ra.id} className="flex items-center justify-between py-2 border-b border-amber-200 last:border-0">
                                        <div>
                                            <p className={`text-sm font-medium ${allReturned ? 'text-brown-light line-through' : 'text-brown'}`}>
                                                {ra.setItemName}
                                            </p>
                                            <p className="text-xs text-amber-700">
                                                {allReturned ? 'all returned' : `${remaining} of ${ra.quantityMissing} still missing`}
                                            </p>
                                        </div>
                                        {allReturned ? (
                                            <span className="text-xs bg-green-50 text-green-700 px-2 py-1">✓ Returned</span>
                                        ) : (
                                            <button
                                                onClick={() => markMissingReturnedMutation.mutate(ra.id)}
                                                disabled={markMissingReturnedMutation.isPending}
                                                className="text-xs border border-amber-400 text-amber-800 px-3 py-1.5 hover:bg-amber-100 transition-colors disabled:opacity-50"
                                            >
                                                Mark 1 Returned
                                            </button>
                                        )}
                                    </div>
                                )
                            })}
                        </div>
                    </div>
                )}

                {/* Complete Booking form — shown when booking is Returned */}
                {booking.status === 'Returned' && (
                    <div className="bg-white border border-gold/20 p-6 space-y-5">
                        <h2 className="font-serif text-lg text-brown">Complete Booking</h2>

                        {/* Deposit summary */}
                        {completion && (
                            <div className="bg-cream-dark px-4 py-3 space-y-1">
                                <div className="flex justify-between text-sm">
                                    <span className="text-brown-light">Security deposit</span>
                                    <span className="text-brown">${completion.depositTotal.toFixed(2)}</span>
                                </div>
                                {completion.deduction > 0 && (
                                    <div className="flex justify-between text-sm">
                                        <span className="text-amber-700">Deduction (damage/missing)</span>
                                        <span className="text-amber-700">−${completion.deduction.toFixed(2)}</span>
                                    </div>
                                )}
                                <div className="flex justify-between text-sm font-semibold border-t border-gold/20 pt-1 mt-1">
                                    <span className="text-brown">Deposit to refund</span>
                                    <span className="text-brown">${completion.refund.toFixed(2)}</span>
                                </div>
                            </div>
                        )}

                        {hasPendingMissing && (
                            <p className="text-xs text-amber-700 bg-amber-50 border border-amber-200 px-3 py-2">
                                {allMissingAssessments.filter(ra => ra.quantityCustomerReturned < ra.quantityMissing).length} component(s) still missing — deposit will be deducted for unreturned items if you complete now.
                            </p>
                        )}

                        <div className="space-y-3">
                            <div>
                                <label className="block text-xs font-semibold text-brown-light uppercase tracking-widest mb-1.5">
                                    Completion Notes
                                </label>
                                <textarea
                                    rows={3}
                                    placeholder="Any notes about the return condition, customer feedback, etc. (optional)"
                                    value={completionNotes}
                                    onChange={e => setCompletionNotes(e.target.value)}
                                    className="w-full border border-gold/20 px-3 py-2 text-sm text-brown placeholder-brown-light/50 focus:outline-none focus:border-gold/50 resize-none"
                                />
                            </div>
                            <div>
                                <label className="block text-xs font-semibold text-brown-light uppercase tracking-widest mb-1.5">
                                    Return Photos <span className="font-normal normal-case">(optional)</span>
                                </label>
                                <label className={`flex items-center gap-3 border border-dashed border-gold/40 px-4 py-3 cursor-pointer hover:border-gold/70 transition-colors ${uploadingPhoto ? 'opacity-50 pointer-events-none' : ''}`}>
                                    <input
                                        type="file"
                                        accept="image/jpeg,image/png,image/webp"
                                        multiple
                                        className="sr-only"
                                        onChange={handlePhotoUpload}
                                        disabled={uploadingPhoto}
                                    />
                                    <span className="text-sm text-brown-light">
                                        {uploadingPhoto ? 'Uploading…' : 'Choose photos'}
                                    </span>
                                    <span className="text-xs text-brown-light/60">JPEG, PNG, WebP</span>
                                </label>
                                {uploadError && (
                                    <p className="text-xs text-red-600 mt-1">{uploadError}</p>
                                )}
                                {uploadedPhotoUrls.length > 0 && (
                                    <div className="grid grid-cols-4 gap-2 mt-3">
                                        {uploadedPhotoUrls.map((url, i) => (
                                            <div key={i} className="relative group">
                                                <img src={url} alt={`Return photo ${i + 1}`} className="w-full aspect-square object-cover border border-gold/20" />
                                                <button
                                                    type="button"
                                                    onClick={() => setUploadedPhotoUrls(prev => prev.filter((_, idx) => idx !== i))}
                                                    className="absolute top-1 right-1 bg-black/50 text-white text-xs w-5 h-5 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity"
                                                >
                                                    ×
                                                </button>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>
                        </div>

                        <div className="pt-1 space-y-2">
                            <button
                                onClick={() => completeMutation.mutate()}
                                disabled={completeMutation.isPending}
                                className="bg-brown hover:bg-brown-mid text-cream text-sm px-6 py-2.5 transition-colors disabled:opacity-50"
                            >
                                {completeMutation.isPending ? 'Completing…' : 'Complete Booking'}
                            </button>
                            {completeMutation.isError && (
                                <p className="text-xs text-red-600">{completeMutation.error.message}</p>
                            )}
                        </div>
                    </div>
                )}

                {/* Completed summary */}
                {booking.status === 'Completed' && booking.depositAmountKept !== undefined && (
                    <div className="bg-white border border-gold/20 p-6">
                        <h2 className="font-serif text-lg text-brown mb-4">Completion Summary</h2>
                        <div className="space-y-2 text-sm">
                            {booking.depositAmountKept != null && booking.depositAmountKept > 0 && (
                                <div className="flex justify-between">
                                    <span className="text-brown-light">Deposit kept (damage/missing)</span>
                                    <span className="text-amber-700">${booking.depositAmountKept.toFixed(2)}</span>
                                </div>
                            )}
                            {booking.completionNotes && (
                                <div className="pt-2 border-t border-gold/10">
                                    <p className="text-xs font-semibold text-brown-light uppercase tracking-widest mb-1">Notes</p>
                                    <p className="text-brown-mid italic text-sm">"{booking.completionNotes}"</p>
                                </div>
                            )}
                        </div>
                    </div>
                )}

                {/* Booking details */}
                <div className="bg-white border border-gold/20 p-6">
                    <h2 className="font-serif text-lg text-brown mb-5">Details</h2>
                    <div className="grid grid-cols-2 gap-x-8 gap-y-4 text-sm">
                        <Detail label="Reservation Date" value={
                            new Date(booking.reservationDate + 'T00:00:00').toLocaleDateString('en-AU', {
                                day: 'numeric', month: 'long', year: 'numeric'
                            })
                        } />
                        <Detail label="Order Number" value={order?.orderNumber ?? '…'} />
                        {booking.confirmedAt && (
                            <Detail label="Confirmed" value={new Date(booking.confirmedAt).toLocaleString('en-AU')} />
                        )}
                        {booking.checkedOutAt && (
                            <Detail label="Checked Out" value={new Date(booking.checkedOutAt).toLocaleString('en-AU')} />
                        )}
                        {booking.returnedAt && (
                            <Detail label="Returned" value={new Date(booking.returnedAt).toLocaleString('en-AU')} />
                        )}
                        {booking.completedAt && (
                            <Detail label="Completed" value={new Date(booking.completedAt).toLocaleString('en-AU')} />
                        )}
                        {booking.notes && (
                            <div className="col-span-2">
                                <Detail label="Notes" value={booking.notes} />
                            </div>
                        )}
                    </div>
                </div>

                {/* Items — inline return form or read-only */}
                <div className="bg-white border border-gold/20 p-6">
                    <h2 className="font-serif text-lg text-brown mb-5">Items</h2>

                    {showReturnForm ? (
                        <div className="space-y-6">
                            {/* Live deposit calculation */}
                            <div className="bg-cream-dark px-4 py-3 flex justify-between items-center">
                                <span className="text-xs font-semibold text-brown-light uppercase tracking-widest">Deposit deduction</span>
                                <div className="text-right">
                                    <p className="font-serif text-brown">
                                        ${currentDeduction.toFixed(2)} kept / ${Math.max(0, depositTotal - currentDeduction).toFixed(2)} returned
                                    </p>
                                    {hasMissingInForm && (
                                        <p className="text-xs text-amber-700 mt-0.5">Order will enter pending state for missing items</p>
                                    )}
                                </div>
                            </div>

                            {booking.bookingItems.map(bi => {
                                const setItems: SetItemDetail[] = booking.setItemsByProduct?.[bi.productId] ?? []
                                return (
                                    <div key={bi.id} className="border border-gold/20 p-4 space-y-4">
                                        <div>
                                            <p className="font-serif text-brown">{bi.productName}</p>
                                            {bi.productColour && <p className="text-xs text-brown-light">{bi.productColour}</p>}
                                            <p className="text-xs text-brown-light font-mono">Set {bi.productSetName}</p>
                                        </div>
                                        {setItems.length === 0 ? (
                                            <p className="text-xs text-brown-light italic">No components configured for this product.</p>
                                        ) : (
                                            <div className="space-y-3">
                                                <div className="grid grid-cols-4 gap-2 text-xs font-semibold text-brown-light uppercase tracking-widest">
                                                    <span>Component</span>
                                                    <span className="text-center">Good</span>
                                                    <span className="text-center text-amber-700">Damaged</span>
                                                    <span className="text-center text-red-600">Missing</span>
                                                </div>
                                                {setItems.map(si => {
                                                    const counts = assessmentState[bi.id]?.[si.id] ?? { good: si.quantity, damaged: 0, missing: 0 }
                                                    const total = counts.good + counts.damaged + counts.missing
                                                    const valid = total === si.quantity
                                                    return (
                                                        <div key={si.id} className="grid grid-cols-4 gap-2 items-center">
                                                            <div>
                                                                <p className="text-sm text-brown">{si.name}</p>
                                                                <p className="text-xs text-brown-light">qty {si.quantity} × ${si.depositValuePerUnit.toFixed(2)}</p>
                                                            </div>
                                                            <input
                                                                type="number" min="0" max={si.quantity}
                                                                value={counts.good}
                                                                onChange={e => updateCount(bi.id, si.id, 'good', parseInt(e.target.value) || 0)}
                                                                className={`border px-2 py-1.5 text-sm text-center w-full ${valid ? 'border-gold/20' : 'border-red-300'}`}
                                                            />
                                                            <input
                                                                type="number" min="0" max={si.quantity}
                                                                value={counts.damaged}
                                                                onChange={e => updateCount(bi.id, si.id, 'damaged', parseInt(e.target.value) || 0)}
                                                                className={`border px-2 py-1.5 text-sm text-center w-full ${valid ? 'border-gold/20' : 'border-red-300'}`}
                                                            />
                                                            <input
                                                                type="number" min="0" max={si.quantity}
                                                                value={counts.missing}
                                                                onChange={e => updateCount(bi.id, si.id, 'missing', parseInt(e.target.value) || 0)}
                                                                className={`border px-2 py-1.5 text-sm text-center w-full ${valid ? 'border-gold/20' : 'border-red-300'}`}
                                                            />
                                                        </div>
                                                    )
                                                })}
                                            </div>
                                        )}
                                        <div>
                                            <input
                                                type="text"
                                                placeholder="Return notes (optional)"
                                                value={returnNotes[bi.id] ?? ''}
                                                onChange={e => setReturnNotes(prev => ({ ...prev, [bi.id]: e.target.value }))}
                                                className="w-full border border-gold/20 px-3 py-2 text-sm text-brown placeholder-brown-light/50 focus:outline-none focus:border-gold/50"
                                            />
                                        </div>
                                    </div>
                                )
                            })}

                            <div className="flex gap-3 pt-2">
                                <button
                                    onClick={() => returnMutation.mutate()}
                                    disabled={returnMutation.isPending || !allTotalsValid}
                                    title={!allTotalsValid ? 'Component quantities must sum to total for each item' : undefined}
                                    className="bg-brown text-cream text-sm px-5 py-2 hover:bg-brown-mid transition-colors disabled:opacity-50"
                                >
                                    {returnMutation.isPending ? 'Saving…' : 'Confirm Return'}
                                </button>
                                <button
                                    onClick={() => setShowReturnForm(false)}
                                    className="text-sm text-brown-light hover:text-brown transition-colors px-3"
                                >
                                    Cancel
                                </button>
                            </div>
                            {returnMutation.isError && (
                                <p className="text-xs text-red-600">{returnMutation.error.message}</p>
                            )}
                        </div>
                    ) : (
                        <div className="space-y-3">
                            {booking.bookingItems.map(item => (
                                <div key={item.id} className="py-3 border-b border-gold/10 last:border-0">
                                    <div className="flex items-start justify-between mb-2">
                                        <div>
                                            <p className="font-serif text-brown">{item.productName}</p>
                                            {item.productColour && (
                                                <p className="text-xs text-brown-light mt-0.5">{item.productColour}</p>
                                            )}
                                            <p className="text-xs text-brown-light font-mono mt-0.5">
                                                {item.productSetName ?? item.productSetId.slice(0, 8)}
                                            </p>
                                            {item.returnNotes && (
                                                <p className="text-xs text-brown-mid mt-1 italic">"{item.returnNotes}"</p>
                                            )}
                                        </div>
                                    </div>
                                    {item.returnAssessments && item.returnAssessments.length > 0 && (
                                        <div className="mt-2 space-y-1 pl-2 border-l-2 border-gold/20">
                                            {item.returnAssessments.map(ra => (
                                                <div key={ra.id} className="flex items-center gap-3 text-xs text-brown-light">
                                                    <span className="w-32">{ra.setItemName}</span>
                                                    {ra.quantityGood > 0 && <span className="text-green-700">{ra.quantityGood} good</span>}
                                                    {ra.quantityDamaged > 0 && <span className="text-amber-700">{ra.quantityDamaged} damaged</span>}
                                                    {ra.quantityMissing > 0 && <span className="text-red-600">{ra.quantityMissing} missing</span>}
                                                    {ra.replacedAt && <span className="text-brown-light">✓ replaced</span>}
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                {/* Order details */}
                {order && (
                    <div className="bg-white border border-gold/20 p-6">
                        <h2 className="font-serif text-lg text-brown mb-5">Order</h2>
                        <div className="grid grid-cols-2 gap-x-8 gap-y-4 text-sm mb-6">
                            <Detail label="Order Number" value={order.orderNumber} />
                            <Detail label="Status" value={order.status} />
                            {order.status === 'Cancelled' && order.cancellationReason && (
                                <Detail label="Cancellation Reason" value={order.cancellationReason} />
                            )}
                            {(order.status === 'Completed' || order.status === 'Cancelled' || order.status === 'PendingMissingItems') && order.paymentStatus === 'Paid' && (
                                <Detail label="Refund" value={
                                    order.refundStatus === 'FullyRefunded' ? `Fully refunded — $${order.amountRefunded?.toFixed(2)}` :
                                    order.refundStatus === 'DepositRefunded' ? `Deposit refunded — $${order.amountRefunded?.toFixed(2)}` :
                                    order.refundStatus === 'DepositPartiallyRefunded' ? `Partial refund — $${order.amountRefunded?.toFixed(2)}` :
                                    'Refund pending'
                                } />
                            )}
                            <Detail label="Customer" value={order.customerName} />
                            <Detail label="Email" value={order.customerEmail} />
                            <Detail label="Phone" value={order.customerPhone} />
                            <Detail label="Event Date" value={
                                new Date(order.reservationDate + 'T00:00:00').toLocaleDateString('en-AU', {
                                    day: 'numeric', month: 'long', year: 'numeric'
                                })
                            } />
                        </div>
                        <div className="border-t border-gold/20 pt-4 space-y-4">
                            {order.orderItems?.map(item => (
                                <div key={item.id} className="flex gap-4 items-center">
                                    <div className="w-12 h-12 bg-cream-dark overflow-hidden shrink-0 flex items-center justify-center">
                                        {item.imageUrl ? (
                                            <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
                                        ) : (
                                            <span className="font-script text-sm text-brown-light">—</span>
                                        )}
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <p className="font-serif text-brown text-sm truncate">{item.name}</p>
                                        {item.colour && <p className="text-xs text-brown-light tracking-wide">{item.colour}</p>}
                                        <p className="text-xs text-brown-mid mt-0.5">Qty {item.quantity} × ${item.unitPrice.toFixed(2)}/day</p>
                                        {item.depositAmount > 0 && (
                                            <p className="text-xs text-brown-light mt-0.5">+${item.depositAmount.toFixed(2)} deposit</p>
                                        )}
                                    </div>
                                    <p className="font-semibold text-brown text-sm shrink-0">${item.total.toFixed(2)}</p>
                                </div>
                            ))}
                            <div className="border-t border-gold/20 pt-4 space-y-2 text-sm">
                                <div className="flex justify-between font-semibold text-brown text-base">
                                    <span>Hire total</span><span>${(order.total - order.depositTotal).toFixed(2)}</span>
                                </div>
                                <div className="flex justify-between text-brown-light text-xs">
                                    <span>GST included (10%)</span><span>${order.tax.toFixed(2)}</span>
                                </div>
                                {order.depositTotal > 0 && (
                                    <div className="flex justify-between text-brown-mid pt-2 border-t border-gold/20">
                                        <div>
                                            <span>Security deposit</span>
                                            <p className="text-xs text-brown-light mt-0.5">Refundable subject to item condition</p>
                                        </div>
                                        <span>${order.depositTotal.toFixed(2)}</span>
                                    </div>
                                )}
                                <div className="flex justify-between font-semibold text-brown text-base pt-2 border-t border-gold/20">
                                    <span>Total</span><span>${order.total.toFixed(2)}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    )
}

function ActionButton({ label, onClick, isPending }: { label: string; onClick: () => void; isPending: boolean }) {
    return (
        <button
            onClick={onClick}
            disabled={isPending}
            className="text-xs tracking-widest uppercase border border-brown/30 text-brown px-4 py-2 hover:bg-cream-dark transition-colors disabled:opacity-50"
        >
            {isPending ? '…' : label}
        </button>
    )
}

function Detail({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
    return (
        <div>
            <p className="text-xs font-semibold text-brown-light uppercase tracking-widest mb-1">{label}</p>
            <p className={`text-brown font-medium ${mono ? 'font-mono text-xs break-all' : ''}`}>{value}</p>
        </div>
    )
}
