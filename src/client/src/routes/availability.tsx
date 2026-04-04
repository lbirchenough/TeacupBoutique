import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { useState, useEffect } from 'react'
import { inventoryApi } from '../lib/inventoryApi'
import { cartStore } from '../lib/cartStore'
import { useCart } from '../lib/useCart'
import type { ProductAvailabilityDto, ProductDetail } from '../lib/types'

export const Route = createFileRoute('/availability')({
    component: AvailabilityPage,
})

const today = new Date().toISOString().split('T')[0]

function AvailabilityPage() {
    const navigate = useNavigate()
    const { reservationDate: cartReservationDate, items: cartItems } = useCart()

    const [selectedDate, setSelectedDate] = useState(() => cartStore.getReservationDate() ?? '')
    const [draftDate, setDraftDate] = useState(() => cartStore.getReservationDate() ?? '')
    const [quantities, setQuantities] = useState<Record<string, number>>({})
    const [modalProduct, setModalProduct] = useState<ProductAvailabilityDto | null>(null)
    const [showConflictPrompt, setShowConflictPrompt] = useState(false)

    const isUpdatingCart = !!selectedDate && selectedDate === cartReservationDate && cartItems.length > 0

    const { data, isPending, isError } = useQuery<ProductAvailabilityDto[]>({
        queryKey: ['availability', selectedDate],
        queryFn: () => inventoryApi.getAvailability(selectedDate),
        enabled: !!selectedDate,
    })

    // When date changes: if it matches cart date, populate from cart; otherwise clear
    useEffect(() => {
        const cartDate = cartStore.getReservationDate()
        const items = cartStore.getItems()
        if (selectedDate && selectedDate === cartDate && items.length > 0) {
            setQuantities(items.reduce((acc, item) => ({ ...acc, [item.productId]: item.quantity }), {}))
        } else {
            setQuantities({})
        }
    }, [selectedDate])

    function setQty(productId: string, delta: number, max: number) {
        setQuantities(prev => {
            const current = prev[productId] ?? 0
            const next = Math.min(Math.max(0, current + delta), max)
            if (next === 0) {
                const { [productId]: _, ...rest } = prev
                return rest
            }
            return { ...prev, [productId]: next }
        })
    }

    const selectedEntries = Object.entries(quantities).filter(([, q]) => q > 0)
    const totalSelected = selectedEntries.reduce((sum, [, q]) => sum + q, 0)
    const totalCost = selectedEntries.reduce((sum, [id, q]) => {
        const product = data?.find(p => p.productId === id)
        return sum + (product?.pricePerDay ?? 0) * q
    }, 0)
    const totalServings = selectedEntries.reduce((sum, [id, q]) => {
        const product = data?.find(p => p.productId === id)
        return sum + (product?.servings ?? 0) * q
    }, 0)

    function commitToCart() {
        if (isUpdatingCart) {
            cartStore.clear()
        }
        selectedEntries.forEach(([productId, qty]) => {
            const product = data?.find(p => p.productId === productId)
            if (!product) return
            cartStore.addItem({
                productId,
                name: product.name,
                pricePerDay: product.pricePerDay,
                depositAmount: product.depositAmount,
                servings: product.servings,
                quantity: qty,
                imageUrl: product.featuredPhotoUrl ?? null,
            })
        })
        cartStore.setReservationDate(selectedDate)
        navigate({ to: '/cart' })
    }

    function handleAddToCart() {
        const existingDate = cartStore.getReservationDate()
        const hasConflict = existingDate && existingDate !== selectedDate && cartStore.getItems().length > 0
        if (hasConflict) {
            setShowConflictPrompt(true)
            return
        }
        commitToCart()
    }

    function handleReplaceCart() {
        cartStore.clear()
        setShowConflictPrompt(false)
        commitToCart()
    }

    return (
        <div className="pb-32">
            {/* Step 1 — header (same height as other pages) */}
            <div className="text-center px-6 pt-16 pb-10">
                <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Step 1</p>
                <h1 className="font-script text-6xl text-brown">When is your event?</h1>
                <p className="text-brown-mid mt-4 text-sm max-w-xs mx-auto leading-relaxed">
                    {selectedDate
                        ? new Date(selectedDate + 'T00:00:00').toLocaleDateString('en-AU', {
                            weekday: 'long', day: 'numeric', month: 'long', year: 'numeric',
                          })
                        : 'Select your special date to check availability'}
                </p>
                {selectedDate && (
                    <button
                        onClick={() => { setSelectedDate(''); setDraftDate('') }}
                        className="text-xs text-brown-light hover:text-brown underline underline-offset-2 transition-colors mt-3"
                    >
                        Change date
                    </button>
                )}
                <div className="flex items-center justify-center gap-3 mt-5">
                    <div className="h-px w-12 bg-gold/40" />
                    <div className="w-1.5 h-1.5 rounded-full bg-gold/60" />
                    <div className="h-px w-12 bg-gold/40" />
                </div>
            </div>

            {/* Date picker — below header, hidden once date chosen */}
            {!selectedDate && (
                <div className="flex justify-center px-6 pb-16">
                    <div className="w-full max-w-sm">
                        <div className="relative">
                            <div className="absolute inset-y-0 left-4 flex items-center pointer-events-none">
                                <svg className="w-5 h-5 text-gold" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                                    <path strokeLinecap="round" strokeLinejoin="round" d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 012.25-2.25h13.5A2.25 2.25 0 0121 7.5v11.25m-18 0A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75m-18 0v-7.5A2.25 2.25 0 015.25 9h13.5A2.25 2.25 0 0121 11.25v7.5" />
                                </svg>
                            </div>
                            <input
                                type="date"
                                min={today}
                                value={draftDate}
                                onChange={e => setDraftDate(e.target.value)}
                                className="w-full border-2 border-gold bg-white pl-12 pr-4 py-4 text-brown focus:outline-none focus:border-gold/80 text-base"
                            />
                        </div>
                        <button
                            disabled={!draftDate}
                            onClick={() => setSelectedDate(draftDate)}
                            className="w-full mt-3 bg-brown hover:bg-brown-mid disabled:opacity-40 text-cream py-3.5 text-xs font-semibold tracking-[0.15em] uppercase transition-colors"
                        >
                            Check Availability
                        </button>
                    </div>
                </div>
            )}

            {selectedDate && isPending && (
                <div className="text-center py-20">
                    <p className="font-script text-3xl text-brown-light">Loading availability…</p>
                </div>
            )}

            {selectedDate && isError && (
                <div className="text-center py-20">
                    <p className="text-red-500">Failed to load availability. Please try again.</p>
                </div>
            )}

            {data && (
                <div className="max-w-4xl mx-auto px-6 pt-6 pb-10">
                    {/* Step 2 header */}
                    <div className="text-center mb-10">
                        <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Step 2</p>
                        <h2 className="font-script text-6xl text-brown">Select Your Items</h2>
                        <p className="text-brown-mid mt-4 text-sm max-w-xs mx-auto leading-relaxed">
                            Choose the sets you'd like to hire for your event
                        </p>
                        <div className="flex items-center justify-center gap-3 mt-5">
                            <div className="h-px w-12 bg-gold/40" />
                            <div className="w-1.5 h-1.5 rounded-full bg-gold/60" />
                            <div className="h-px w-12 bg-gold/40" />
                        </div>
                    </div>

                    <div className="flex flex-col gap-3">
                        {data.map(product => (
                            <ProductAvailabilityCard
                                key={product.productId}
                                product={product}
                                quantity={quantities[product.productId] ?? 0}
                                onQtyChange={(delta) => setQty(product.productId, delta, product.available)}
                                onViewDetails={() => setModalProduct(product)}
                            />
                        ))}
                    </div>
                </div>
            )}

            {/* Sticky bar */}
            {totalSelected > 0 && (
                <div className="fixed bottom-0 left-0 right-0 bg-brown border-t border-gold/20 shadow-2xl z-40">
                    <div className="max-w-4xl mx-auto px-6 py-4 flex items-center justify-between gap-4">
                        <div className="min-w-0">
                            <p className="font-semibold text-cream">
                                {totalSelected} item{totalSelected !== 1 ? 's' : ''} &middot; ${totalCost.toFixed(2)}
                                {totalServings > 0 && (
                                    <span className="font-normal text-cream/60"> &middot; serves {totalServings} people</span>
                                )}
                            </p>
                            <p className="text-xs text-cream/50 truncate mt-0.5">
                                {selectedEntries.map(([id, q]) => {
                                    const name = data?.find(p => p.productId === id)?.name ?? id
                                    return `${q}× ${name}`
                                }).join(', ')}
                            </p>
                        </div>
                        <button
                            onClick={handleAddToCart}
                            className="shrink-0 bg-gold hover:bg-gold-light text-brown font-semibold px-8 py-3 text-xs tracking-[0.15em] uppercase transition-colors"
                        >
                            {isUpdatingCart ? 'Update Cart →' : 'Add to Cart →'}
                        </button>
                    </div>
                </div>
            )}

            {/* Date conflict modal */}
            {showConflictPrompt && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
                    <div className="absolute inset-0 bg-brown/60" onClick={() => setShowConflictPrompt(false)} />
                    <div className="relative bg-cream rounded-none shadow-2xl w-full max-w-sm p-8">
                        <h3 className="font-serif text-xl text-brown mb-3">Different date selected</h3>
                        <p className="text-sm text-brown-mid mb-2">
                            Your cart already has items booked for{' '}
                            <span className="font-semibold">
                                {new Date((cartStore.getReservationDate() ?? '') + 'T00:00:00').toLocaleDateString('en-AU', { day: 'numeric', month: 'long', year: 'numeric' })}
                            </span>.
                        </p>
                        <p className="text-sm text-brown-mid mb-7">
                            Orders must be for a single date — those items will be removed if you continue.
                        </p>
                        <div className="flex gap-3">
                            <button
                                onClick={() => { setShowConflictPrompt(false); setQuantities({}) }}
                                className="flex-1 border border-brown/30 text-brown font-medium py-2.5 text-sm hover:bg-cream-dark transition-colors"
                            >
                                Keep current cart
                            </button>
                            <button
                                onClick={handleReplaceCart}
                                className="flex-1 bg-brown hover:bg-brown-mid text-cream font-medium py-2.5 text-sm transition-colors"
                            >
                                Replace cart
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Product detail modal */}
            {modalProduct && (
                <ProductDetailModal
                    product={modalProduct}
                    quantity={quantities[modalProduct.productId] ?? 0}
                    onQtyChange={(delta) => setQty(modalProduct.productId, delta, modalProduct.available)}
                    onClose={() => setModalProduct(null)}
                />
            )}
        </div>
    )
}

// ─── Card ─────────────────────────────────────────────────────────────────────

function ProductAvailabilityCard({
    product, quantity, onQtyChange, onViewDetails,
}: {
    product: ProductAvailabilityDto
    quantity: number
    onQtyChange: (delta: number) => void
    onViewDetails: () => void
}) {
    const isUnavailable = product.available === 0
    const ratio = product.total === 0 ? 0 : product.available / product.total

    const badge =
        product.total === 0
            ? { label: 'No stock', colour: 'bg-cream-dark text-brown-light border border-brown/10' }
            : isUnavailable
            ? { label: 'Fully booked', colour: 'bg-red-50 text-red-700 border border-red-200' }
            : ratio < 0.5
            ? { label: `${product.available} of ${product.total} left`, colour: 'bg-amber-50 text-amber-700 border border-amber-200' }
            : { label: `${product.available} of ${product.total} free`, colour: 'bg-green-50 text-green-700 border border-green-200' }

    return (
        <div className={`bg-white border overflow-hidden flex flex-row transition-opacity ${isUnavailable ? 'opacity-50' : ''} ${quantity > 0 ? 'border-gold' : 'border-gold/20'}`}>
            {/* Thumbnail */}
            <button
                onClick={onViewDetails}
                className="w-24 sm:w-32 h-24 sm:h-32 bg-cream-dark shrink-0 overflow-hidden self-center"
            >
                {product.featuredPhotoUrl ? (
                    <img src={product.featuredPhotoUrl} alt={product.name} className="w-full h-full object-cover hover:scale-105 transition-transform duration-200" />
                ) : (
                    <span className="flex items-center justify-center w-full h-full font-script text-brown-light text-sm">No image</span>
                )}
            </button>

            {/* Content */}
            <div className="flex flex-1 items-center gap-4 px-5 py-4 min-w-0">
                <div className="flex-1 min-w-0">
                    <button
                        onClick={onViewDetails}
                        className="font-serif text-brown leading-tight text-left hover:text-gold transition-colors truncate block w-full text-base"
                    >
                        {product.name}
                    </button>
                    <p className="text-sm text-brown-mid mt-0.5">
                        ${product.pricePerDay.toFixed(2)}
                        {product.servings > 0 && <span className="ml-2 text-brown-light">· serves {product.servings} people</span>}
                    </p>
                    <span className={`inline-block mt-2 text-xs font-medium px-2.5 py-0.5 rounded-full ${badge.colour}`}>
                        {badge.label}
                    </span>
                </div>

                {/* Controls */}
                <div className="shrink-0 flex items-center gap-3">
                    {!isUnavailable && (
                        quantity === 0 ? (
                            <button
                                onClick={() => onQtyChange(1)}
                                className="border border-brown text-brown text-xs font-semibold tracking-widest uppercase px-4 py-2 hover:bg-brown hover:text-cream transition-colors"
                            >
                                + Add
                            </button>
                        ) : (
                            <div className="flex items-center gap-2">
                                <button
                                    onClick={() => onQtyChange(-1)}
                                    className="w-8 h-8 border border-brown/30 text-brown hover:bg-cream-dark font-medium transition-colors"
                                >
                                    −
                                </button>
                                <span className="font-semibold text-brown w-6 text-center">{quantity}</span>
                                <button
                                    onClick={() => onQtyChange(1)}
                                    disabled={quantity >= product.available}
                                    className="w-8 h-8 border border-brown/30 text-brown hover:bg-cream-dark font-medium transition-colors disabled:opacity-30"
                                >
                                    +
                                </button>
                            </div>
                        )
                    )}
                    <button
                        onClick={onViewDetails}
                        className="text-xs text-brown-light hover:text-brown transition-colors"
                    >
                        Details
                    </button>
                </div>
            </div>
        </div>
    )
}

// ─── Modal ────────────────────────────────────────────────────────────────────

function ProductDetailModal({
    product, quantity, onQtyChange, onClose,
}: {
    product: ProductAvailabilityDto
    quantity: number
    onQtyChange: (delta: number) => void
    onClose: () => void
}) {
    const { data: detail, isPending } = useQuery<ProductDetail>({
        queryKey: ['product', product.productId],
        queryFn: () => inventoryApi.getProduct(product.productId),
    })

    const isUnavailable = product.available === 0
    const [photoIndex, setPhotoIndex] = useState(0)
    const photos = detail?.photos ?? []
    const displayPhoto = photos[photoIndex]?.url ?? product.featuredPhotoUrl

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-brown/60" onClick={onClose} />

            <div className="relative bg-cream w-full max-w-lg max-h-[90vh] overflow-y-auto shadow-2xl">
                <button
                    onClick={onClose}
                    className="absolute top-4 right-4 z-10 w-8 h-8 flex items-center justify-center text-brown-mid hover:text-brown text-lg"
                >
                    ✕
                </button>

                {/* Photo */}
                <div className="h-60 bg-cream-dark overflow-hidden flex items-center justify-center">
                    {displayPhoto ? (
                        <img src={displayPhoto} alt={product.name} className="w-full h-full object-cover" />
                    ) : (
                        <span className="font-script text-2xl text-brown-light">No image</span>
                    )}
                </div>

                {/* Photo strip */}
                {photos.length > 1 && (
                    <div className="flex gap-2 px-5 py-3 overflow-x-auto bg-white border-b border-gold/20">
                        {photos.map((ph, i) => (
                            <button
                                key={ph.id}
                                onClick={() => setPhotoIndex(i)}
                                className={`w-12 h-12 overflow-hidden shrink-0 border-2 transition-colors ${i === photoIndex ? 'border-gold' : 'border-transparent'}`}
                            >
                                <img src={ph.url} alt="" className="w-full h-full object-cover" />
                            </button>
                        ))}
                    </div>
                )}

                <div className="p-6">
                    <h2 className="font-serif text-2xl text-brown mb-1">{product.name}</h2>
                    {detail?.colour && <p className="text-xs tracking-widest uppercase text-gold mb-3">{detail.colour}</p>}
                    <p className="text-xl font-semibold text-brown mb-5">${product.pricePerDay.toFixed(2)}</p>

                    {isPending && <p className="text-sm text-brown-light mb-4">Loading details…</p>}

                    {detail?.description && (
                        <p className="text-sm text-brown-mid leading-relaxed mb-4">{detail.description}</p>
                    )}
                    {detail?.contents && (
                        <div className="mb-4">
                            <p className="text-xs font-semibold text-brown-light uppercase tracking-widest mb-1">Contents</p>
                            <p className="text-sm text-brown-mid">{detail.contents}</p>
                        </div>
                    )}
                    {!!detail?.servings && (
                        <p className="text-sm text-brown-mid mb-5">Serves {detail.servings} people</p>
                    )}

                    {/* Availability badge */}
                    <div className="mb-6">
                        <span className={`text-xs font-medium px-3 py-1 rounded-full border ${
                            isUnavailable
                                ? 'bg-red-50 text-red-700 border-red-200'
                                : product.available / product.total < 0.5
                                ? 'bg-amber-50 text-amber-700 border-amber-200'
                                : 'bg-green-50 text-green-700 border-green-200'
                        }`}>
                            {isUnavailable ? 'Fully booked' : `${product.available} of ${product.total} available`}
                        </span>
                    </div>

                    {/* Quantity controls */}
                    {!isUnavailable && (
                        <div className="flex items-center gap-4">
                            {quantity === 0 ? (
                                <button
                                    onClick={() => onQtyChange(1)}
                                    className="flex-1 border border-brown text-brown font-semibold py-3 text-xs tracking-widest uppercase hover:bg-brown hover:text-cream transition-colors"
                                >
                                    + Add to Selection
                                </button>
                            ) : (
                                <>
                                    <div className="flex items-center gap-3 border border-brown/30 px-4 py-2.5">
                                        <button onClick={() => onQtyChange(-1)} className="text-brown hover:text-brown-mid font-medium w-5 text-center">−</button>
                                        <span className="font-semibold text-brown w-5 text-center">{quantity}</span>
                                        <button
                                            onClick={() => onQtyChange(1)}
                                            disabled={quantity >= product.available}
                                            className="text-brown hover:text-brown-mid font-medium w-5 text-center disabled:opacity-30"
                                        >+</button>
                                    </div>
                                    <button
                                        onClick={onClose}
                                        className="flex-1 bg-brown hover:bg-brown-mid text-cream font-semibold py-3 text-xs tracking-widest uppercase transition-colors"
                                    >
                                        Done
                                    </button>
                                </>
                            )}
                        </div>
                    )}
                </div>
            </div>
        </div>
    )
}
