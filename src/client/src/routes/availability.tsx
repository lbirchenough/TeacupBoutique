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
    const [quantities, setQuantities] = useState<Record<string, number>>({})
    const [modalProduct, setModalProduct] = useState<ProductAvailabilityDto | null>(null)
    const [showConflictPrompt, setShowConflictPrompt] = useState(false)

    const isUpdatingCart = !!selectedDate && selectedDate === cartReservationDate && cartItems.length > 0

    const { data, isPending, isError } = useQuery<ProductAvailabilityDto[]>({
        queryKey: ['availability', selectedDate],
        queryFn: () => inventoryApi.getAvailability(selectedDate),
        enabled: !!selectedDate,
    })

    // When date changes (including on mount): if it matches cart date, populate from cart; otherwise clear
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
        <div className="max-w-5xl mx-auto px-4 py-10 pb-32">
            <h1 className="text-2xl font-bold text-gray-900 mb-1">Check Availability</h1>
            <p className="text-gray-500 mb-6">Pick your event date, then select what you need.</p>

            {/* Date picker */}
            <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm mb-8 flex items-center gap-4">
                <label className="text-sm font-medium text-gray-700 shrink-0">Event date</label>
                {selectedDate ? (
                    <div className="flex items-center gap-3">
                        <span className="font-medium text-gray-900">
                            {new Date(selectedDate + 'T00:00:00').toLocaleDateString('en-AU', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}
                        </span>
                        <button
                            onClick={() => setSelectedDate('')}
                            className="text-xs text-gray-400 hover:text-gray-600 underline"
                        >
                            Change date
                        </button>
                    </div>
                ) : (
                    <input
                        type="date"
                        min={today}
                        value={selectedDate}
                        onChange={e => setSelectedDate(e.target.value)}
                        className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                )}
            </div>

            {!selectedDate && (
                <div className="text-center py-20 text-gray-400">
                    <p className="text-lg">Select a date above to see availability</p>
                </div>
            )}

            {selectedDate && isPending && (
                <div className="text-center py-20 text-gray-400">Loading availability…</div>
            )}

            {selectedDate && isError && (
                <div className="text-center py-20 text-red-500">Failed to load availability.</div>
            )}

            {data && (
                <>
                    <p className="text-sm text-gray-500 mb-4">
                        {data.length} products for{' '}
                        {new Date(selectedDate + 'T00:00:00').toLocaleDateString('en-AU', {
                            weekday: 'long', day: 'numeric', month: 'long', year: 'numeric',
                        })}
                    </p>
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
                </>
            )}

            {/* Sticky add-to-cart bar */}
            {totalSelected > 0 && (
                <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 shadow-lg z-40">
                    <div className="max-w-5xl mx-auto px-4 py-3 flex items-center justify-between gap-4">
                        <div className="min-w-0">
                            <p className="font-semibold text-gray-900">
                                {totalSelected} item{totalSelected !== 1 ? 's' : ''} selected &middot; ${totalCost.toFixed(2)}
                                {totalServings > 0 && <span className="font-normal text-gray-500"> &middot; serves {totalServings} people</span>}
                            </p>
                            <p className="text-xs text-gray-500 truncate mt-0.5">
                                {selectedEntries.map(([id, q]) => {
                                    const name = data?.find(p => p.productId === id)?.name ?? id
                                    return `${q}× ${name}`
                                }).join(', ')}
                            </p>
                        </div>
                        <button
                            onClick={handleAddToCart}
                            className="shrink-0 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold px-6 py-2.5 rounded-lg transition-colors"
                        >
                            {isUpdatingCart ? 'Update cart →' : 'Add to cart →'}
                        </button>
                    </div>
                </div>
            )}

            {/* Date conflict modal */}
            {showConflictPrompt && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
                    <div className="absolute inset-0 bg-black/50" onClick={() => setShowConflictPrompt(false)} />
                    <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-sm p-6">
                        <h3 className="font-bold text-gray-900 mb-2">Different date selected</h3>
                        <p className="text-sm text-gray-600 mb-2">
                            Your cart already has items booked for{' '}
                            <span className="font-medium">
                                {new Date((cartStore.getReservationDate() ?? '') + 'T00:00:00').toLocaleDateString('en-AU', { day: 'numeric', month: 'long', year: 'numeric' })}
                            </span>.
                        </p>
                        <p className="text-sm text-gray-600 mb-5">
                            Orders must be for a single date — those items will be removed.
                            Please place a separate order if you need rentals for multiple dates.
                        </p>
                        <div className="flex gap-3">
                            <button
                                onClick={() => { setShowConflictPrompt(false); setQuantities({}) }}
                                className="flex-1 border border-gray-300 text-gray-700 font-medium py-2 rounded-lg hover:bg-gray-50 text-sm"
                            >
                                Keep current cart
                            </button>
                            <button
                                onClick={handleReplaceCart}
                                className="flex-1 bg-indigo-600 hover:bg-indigo-700 text-white font-medium py-2 rounded-lg text-sm"
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

// ─── Card ────────────────────────────────────────────────────────────────────

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
            ? { label: 'No stock', colour: 'bg-gray-100 text-gray-500' }
            : isUnavailable
            ? { label: 'Fully booked', colour: 'bg-red-100 text-red-700' }
            : ratio < 0.5
            ? { label: `${product.available} of ${product.total} left`, colour: 'bg-amber-100 text-amber-700' }
            : { label: `${product.available} of ${product.total} free`, colour: 'bg-green-100 text-green-700' }

    return (
        <div className={`bg-white border rounded-xl overflow-hidden shadow-sm flex flex-row transition-opacity ${isUnavailable ? 'opacity-50' : ''} ${quantity > 0 ? 'border-indigo-300 ring-1 ring-indigo-300' : 'border-gray-200'}`}>
            {/* Thumbnail */}
            <button
                onClick={onViewDetails}
                className="w-24 sm:w-32 bg-gray-100 shrink-0 overflow-hidden"
            >
                {product.featuredPhotoUrl ? (
                    <img src={product.featuredPhotoUrl} alt={product.name} className="w-full h-full object-cover hover:scale-105 transition-transform duration-200" />
                ) : (
                    <span className="flex items-center justify-center w-full h-full text-gray-300 text-xs">No image</span>
                )}
            </button>

            {/* Content */}
            <div className="flex flex-1 items-center gap-4 px-4 py-3 min-w-0">
                {/* Name + badge */}
                <div className="flex-1 min-w-0">
                    <button
                        onClick={onViewDetails}
                        className="font-semibold text-gray-900 leading-tight text-left hover:text-indigo-600 transition-colors truncate block w-full"
                    >
                        {product.name}
                    </button>
                    <p className="text-sm text-gray-500 mt-0.5">
                        ${product.pricePerDay.toFixed(2)}
                        {product.servings > 0 && <span className="ml-2">· serves {product.servings}</span>}
                    </p>
                    <span className={`inline-block mt-1.5 text-xs font-medium px-2 py-0.5 rounded-full ${badge.colour}`}>
                        {badge.label}
                    </span>
                </div>

                {/* Controls */}
                <div className="shrink-0 flex items-center gap-2">
                    {!isUnavailable && (
                        quantity === 0 ? (
                            <button
                                onClick={() => onQtyChange(1)}
                                className="bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
                            >
                                + Add
                            </button>
                        ) : (
                            <div className="flex items-center gap-2">
                                <button
                                    onClick={() => onQtyChange(-1)}
                                    className="w-8 h-8 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-50 font-medium"
                                >
                                    −
                                </button>
                                <span className="font-semibold text-gray-900 w-6 text-center">{quantity}</span>
                                <button
                                    onClick={() => onQtyChange(1)}
                                    disabled={quantity >= product.available}
                                    className="w-8 h-8 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-50 font-medium disabled:opacity-40"
                                >
                                    +
                                </button>
                            </div>
                        )
                    )}
                    <button
                        onClick={onViewDetails}
                        className="text-xs text-gray-400 hover:text-indigo-600 pl-1"
                    >
                        Details
                    </button>
                </div>
            </div>
        </div>
    )
}

// ─── Modal ───────────────────────────────────────────────────────────────────

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
            <div className="absolute inset-0 bg-black/50" onClick={onClose} />

            <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-lg max-h-[90vh] overflow-y-auto">
                <button
                    onClick={onClose}
                    className="absolute top-4 right-4 z-10 w-8 h-8 rounded-full bg-white/80 hover:bg-white flex items-center justify-center text-gray-500 hover:text-gray-900 shadow text-sm"
                >
                    ✕
                </button>

                {/* Photo */}
                <div className="h-56 bg-gray-100 overflow-hidden rounded-t-2xl flex items-center justify-center">
                    {displayPhoto ? (
                        <img src={displayPhoto} alt={product.name} className="w-full h-full object-cover" />
                    ) : (
                        <span className="text-gray-300">No image</span>
                    )}
                </div>

                {/* Photo strip */}
                {photos.length > 1 && (
                    <div className="flex gap-2 px-5 py-2 overflow-x-auto">
                        {photos.map((ph, i) => (
                            <button
                                key={ph.id}
                                onClick={() => setPhotoIndex(i)}
                                className={`w-12 h-12 rounded-md overflow-hidden shrink-0 border-2 transition-colors ${i === photoIndex ? 'border-indigo-500' : 'border-transparent'}`}
                            >
                                <img src={ph.url} alt="" className="w-full h-full object-cover" />
                            </button>
                        ))}
                    </div>
                )}

                <div className="p-5">
                    <h2 className="text-xl font-bold text-gray-900 mb-1">{product.name}</h2>
                    {detail?.colour && <p className="text-sm text-gray-500 mb-2">{detail.colour}</p>}
                    <p className="text-lg font-semibold text-gray-900 mb-4">${product.pricePerDay.toFixed(2)} / day</p>

                    {isPending && <p className="text-sm text-gray-400 mb-4">Loading details…</p>}

                    {detail?.description && (
                        <p className="text-sm text-gray-600 mb-3">{detail.description}</p>
                    )}
                    {detail?.contents && (
                        <div className="mb-3">
                            <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">Contents</p>
                            <p className="text-sm text-gray-700">{detail.contents}</p>
                        </div>
                    )}
                    {!!detail?.servings && (
                        <p className="text-sm text-gray-500 mb-4">Serves {detail.servings}</p>
                    )}

                    {/* Availability badge */}
                    <div className="mb-5">
                        <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${
                            isUnavailable ? 'bg-red-100 text-red-700' :
                            product.available / product.total < 0.5 ? 'bg-amber-100 text-amber-700' :
                            'bg-green-100 text-green-700'
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
                                    className="flex-1 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-2.5 rounded-lg transition-colors"
                                >
                                    + Add to selection
                                </button>
                            ) : (
                                <>
                                    <div className="flex items-center gap-3 border border-gray-300 rounded-lg px-3 py-2">
                                        <button onClick={() => onQtyChange(-1)} className="text-gray-600 hover:text-gray-900 font-medium w-5 text-center">−</button>
                                        <span className="font-semibold w-5 text-center">{quantity}</span>
                                        <button
                                            onClick={() => onQtyChange(1)}
                                            disabled={quantity >= product.available}
                                            className="text-gray-600 hover:text-gray-900 font-medium w-5 text-center disabled:opacity-40"
                                        >+</button>
                                    </div>
                                    <button
                                        onClick={onClose}
                                        className="flex-1 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-2.5 rounded-lg transition-colors"
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
