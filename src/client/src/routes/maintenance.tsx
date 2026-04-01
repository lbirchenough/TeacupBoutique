import { createFileRoute, Link } from '@tanstack/react-router'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { bookingsApi } from '../lib/bookingsApi'
import type { MaintenanceQueueItem, SpareStockItem } from '../lib/types'

export const Route = createFileRoute('/maintenance')({
    component: MaintenancePage,
})

function MaintenancePage() {
    const queryClient = useQueryClient()

    const { data: queue = [], isPending: queuePending } = useQuery<MaintenanceQueueItem[]>({
        queryKey: ['maintenance-queue'],
        queryFn: () => bookingsApi.getMaintenanceQueue(),
        refetchInterval: 30_000,
    })

    const { data: spareStock = [] } = useQuery<SpareStockItem[]>({
        queryKey: ['spare-stock'],
        queryFn: () => bookingsApi.getAllSpareStock(),
    })

    const invalidate = () => {
        queryClient.invalidateQueries({ queryKey: ['maintenance-queue'] })
        queryClient.invalidateQueries({ queryKey: ['spare-stock'] })
    }

    if (queuePending) return (
        <div className="max-w-4xl mx-auto px-6 py-20 text-center">
            <p className="font-serif text-lg text-brown-light">Loading maintenance queue…</p>
        </div>
    )

    return (
        <div>
            <div className="bg-cream-dark border-b border-gold/20 py-16 text-center">
                <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-4">Admin</p>
                <h1 className="font-serif text-3xl text-brown">Maintenance Queue</h1>
                {queue.length > 0 && (
                    <p className="text-brown-light text-sm mt-2">{queue.length} set{queue.length !== 1 ? 's' : ''} in maintenance</p>
                )}
            </div>

            <div className="max-w-4xl mx-auto px-6 py-12 space-y-10">

                {/* Maintenance queue */}
                {queue.length === 0 ? (
                    <div className="bg-white border border-gold/20 p-10 text-center">
                        <p className="font-serif text-lg text-brown-light">No sets in maintenance</p>
                        <p className="text-sm text-brown-light mt-2">All sets are available or retired.</p>
                    </div>
                ) : (
                    <div className="space-y-4">
                        {queue.map(item => (
                            <MaintenanceCard
                                key={item.productSetId}
                                item={item}
                                onUpdate={invalidate}
                            />
                        ))}
                    </div>
                )}

                {/* Spare stock management */}
                <div className="bg-white border border-gold/20 p-6">
                    <h2 className="font-serif text-lg text-brown mb-5">Spare Stock</h2>
                    {spareStock.length === 0 ? (
                        <p className="text-sm text-brown-light italic">No spare stock configured.</p>
                    ) : (
                        <div className="space-y-1">
                            {/* Group by product */}
                            {Object.entries(
                                spareStock.reduce<Record<string, SpareStockItem[]>>((acc, ss) => {
                                    const key = ss.productName
                                    acc[key] = acc[key] ?? []
                                    acc[key].push(ss)
                                    return acc
                                }, {})
                            ).map(([productName, items]) => (
                                <div key={productName} className="py-3 border-b border-gold/10 last:border-0">
                                    <p className="font-serif text-brown text-sm mb-2">{productName}</p>
                                    <div className="space-y-2 pl-3">
                                        {items.map(ss => (
                                            <SpareStockRow
                                                key={ss.setItemId}
                                                item={ss}
                                                onUpdate={invalidate}
                                            />
                                        ))}
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

            </div>
        </div>
    )
}

function MaintenanceCard({ item, onUpdate }: { item: MaintenanceQueueItem; onUpdate: () => void }) {
    const queryClient = useQueryClient()
    const [addStockSetItemId, setAddStockSetItemId] = useState<string | null>(null)
    const [addStockQty, setAddStockQty] = useState('1')

    const markReplacedMutation = useMutation({
        mutationFn: (setItemId: string) => bookingsApi.markComponentReplaced(item.productSetId, setItemId),
        onSuccess: onUpdate,
    })

    const markCleanedMutation = useMutation({
        mutationFn: () => bookingsApi.markCleaned(item.bookingId),
        onSuccess: onUpdate,
    })

    const completeMutation = useMutation({
        mutationFn: () => bookingsApi.releaseSet(item.productSetId),
        onSuccess: () => {
            onUpdate()
            queryClient.invalidateQueries({ queryKey: ['bookings'] })
        },
    })

    const addSpareStockMutation = useMutation({
        mutationFn: ({ setItemId, qty }: { setItemId: string; qty: number }) =>
            bookingsApi.addSpareStock(setItemId, qty),
        onSuccess: () => {
            setAddStockSetItemId(null)
            setAddStockQty('1')
            onUpdate()
        },
    })

    const allComponentsDone = item.components.every(c => c.isReplaced)
    const canComplete = allComponentsDone && item.isCleaned

    return (
        <div className="bg-white border border-gold/20 p-6 space-y-5">
            {/* Header row */}
            <div className="flex items-start justify-between gap-4">
                <div>
                    <p className="font-serif text-brown text-lg">{item.productName} — {item.productSetName}</p>
                    {item.returnedAt && (
                        <p className="text-xs text-brown-light mt-0.5">
                            Returned {new Date(item.returnedAt).toLocaleDateString('en-AU', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' })}
                        </p>
                    )}
                    <Link
                        to="/bookings/$bookingId"
                        params={{ bookingId: item.bookingId }}
                        className="text-xs text-gold hover:text-gold/70 transition-colors mt-0.5 inline-block"
                    >
                        View booking →
                    </Link>
                </div>

                {/* Urgency badge */}
                {item.upcomingBookings.length > 0 && (
                    <div className="bg-red-50 border border-red-200 px-3 py-2 shrink-0 text-right">
                        <p className="text-xs font-semibold text-red-700">
                            {item.upcomingBookings.length} upcoming booking{item.upcomingBookings.length !== 1 ? 's' : ''}
                        </p>
                        {item.upcomingBookings.slice(0, 3).map((ub, i) => (
                            <p key={i} className="text-xs text-red-600 mt-0.5">
                                {new Date(ub.reservationDate + 'T00:00:00').toLocaleDateString('en-AU', { day: 'numeric', month: 'short' })}
                                {' '}({ub.daysUntil === 0 ? 'today' : ub.daysUntil === 1 ? 'tomorrow' : `${ub.daysUntil}d`})
                            </p>
                        ))}
                    </div>
                )}
            </div>

            {/* Components needing replacement */}
            {item.components.length > 0 ? (
                <div className="space-y-2">
                    <p className="text-xs font-semibold text-brown-light uppercase tracking-widest">Components</p>
                    {item.components.map(comp => (
                        <div key={comp.setItemId} className="border border-gold/10 p-3">
                            <div className="flex items-center justify-between">
                                <div>
                                    <p className="text-sm text-brown font-medium">{comp.setItemName}</p>
                                    <p className="text-xs text-brown-light mt-0.5">
                                        {comp.totalDamaged > 0 && `${comp.totalDamaged} damaged`}
                                        {comp.totalDamaged > 0 && comp.totalMissing > 0 && ', '}
                                        {comp.totalMissing > 0 && `${comp.totalMissing} missing`}
                                        {' · '}spare stock: {comp.spareStockAvailable}
                                    </p>
                                </div>
                                {comp.isReplaced ? (
                                    <span className="text-xs bg-green-50 text-green-700 px-2 py-1">✓ Replaced</span>
                                ) : comp.spareStockAvailable >= (comp.totalDamaged + comp.totalMissing) ? (
                                    <button
                                        onClick={() => markReplacedMutation.mutate(comp.setItemId)}
                                        disabled={markReplacedMutation.isPending}
                                        className="text-xs border border-brown/30 text-brown px-3 py-1.5 hover:bg-cream-dark transition-colors disabled:opacity-50"
                                    >
                                        Mark Replaced
                                    </button>
                                ) : (
                                    <span className="text-xs bg-red-50 text-red-600 px-2 py-1">No spare stock</span>
                                )}
                            </div>
                            {!comp.isReplaced && comp.spareStockAvailable < (comp.totalDamaged + comp.totalMissing) && (
                                <div className="mt-2 pt-2 border-t border-gold/10">
                                    {addStockSetItemId === comp.setItemId ? (
                                        <div className="flex items-center gap-2">
                                            <input
                                                type="number" min="1" value={addStockQty}
                                                onChange={e => setAddStockQty(e.target.value)}
                                                className="w-16 border border-gold/20 px-2 py-1 text-sm text-brown text-center"
                                            />
                                            <button
                                                onClick={() => addSpareStockMutation.mutate({ setItemId: comp.setItemId, qty: parseInt(addStockQty) || 1 })}
                                                disabled={addSpareStockMutation.isPending}
                                                className="text-xs bg-brown text-cream px-3 py-1.5 hover:bg-brown-mid transition-colors disabled:opacity-50"
                                            >
                                                {addSpareStockMutation.isPending ? '…' : 'Add Stock'}
                                            </button>
                                            <button onClick={() => setAddStockSetItemId(null)} className="text-xs text-brown-light px-2">Cancel</button>
                                        </div>
                                    ) : (
                                        <button
                                            onClick={() => { setAddStockSetItemId(comp.setItemId); setAddStockQty('1') }}
                                            className="text-xs text-brown-light hover:text-brown underline"
                                        >
                                            + Add spare stock
                                        </button>
                                    )}
                                </div>
                            )}
                            {markReplacedMutation.isError && (
                                <p className="text-xs text-red-600 mt-1">{markReplacedMutation.error.message}</p>
                            )}
                        </div>
                    ))}
                </div>
            ) : (
                <p className="text-sm text-brown-light italic">All items returned in good condition — no replacements needed.</p>
            )}

            {/* Cleaned sign-off */}
            <div className="flex items-center justify-between pt-2 border-t border-gold/10">
                <div>
                    <p className="text-sm font-medium text-brown">Cleaned</p>
                    <p className="text-xs text-brown-light mt-0.5">Confirm the set has been cleaned and is ready to go out</p>
                </div>
                {item.isCleaned ? (
                    <span className="text-xs bg-green-50 text-green-700 px-2 py-1">✓ Cleaned</span>
                ) : (
                    <button
                        onClick={() => markCleanedMutation.mutate()}
                        disabled={markCleanedMutation.isPending}
                        className="text-xs border border-brown/30 text-brown px-3 py-1.5 hover:bg-cream-dark transition-colors disabled:opacity-50"
                    >
                        {markCleanedMutation.isPending ? '…' : 'Mark Cleaned'}
                    </button>
                )}
            </div>

            {/* Complete button */}
            <div className="pt-2 border-t border-gold/20">
                {canComplete ? (
                    <div className="space-y-1">
                        <button
                            onClick={() => completeMutation.mutate()}
                            disabled={completeMutation.isPending}
                            className="text-xs bg-brown hover:bg-brown-mid text-cream px-5 py-2 transition-colors disabled:opacity-50"
                        >
                            {completeMutation.isPending ? 'Releasing…' : 'Release Set to Available'}
                        </button>
                        {completeMutation.isError && (
                            <p className="text-xs text-red-600">{completeMutation.error.message}</p>
                        )}
                    </div>
                ) : (
                    <p className="text-xs text-brown-light italic">
                        {!allComponentsDone && !item.isCleaned
                            ? 'Mark all components replaced and set as cleaned to release.'
                            : !allComponentsDone
                                ? 'Mark all components replaced to release.'
                                : 'Mark set as cleaned to release.'}
                    </p>
                )}
            </div>
        </div>
    )
}

function SpareStockRow({ item, onUpdate }: { item: SpareStockItem; onUpdate: () => void }) {
    const [adding, setAdding] = useState(false)
    const [qty, setQty] = useState('1')

    const addMutation = useMutation({
        mutationFn: (q: number) => bookingsApi.addSpareStock(item.setItemId, q),
        onSuccess: () => { setAdding(false); setQty('1'); onUpdate() },
    })

    const removeMutation = useMutation({
        mutationFn: (q: number) => bookingsApi.removeSpareStock(item.setItemId, q),
        onSuccess: onUpdate,
    })

    return (
        <div className="flex items-center justify-between py-1">
            <span className="text-sm text-brown-mid">{item.setItemName}</span>
            <div className="flex items-center gap-3">
                <span className={`text-sm font-semibold ${item.quantityAvailable === 0 ? 'text-red-600' : 'text-brown'}`}>
                    {item.quantityAvailable}
                </span>
                {adding ? (
                    <div className="flex items-center gap-1">
                        <input
                            type="number" min="1" value={qty}
                            onChange={e => setQty(e.target.value)}
                            className="w-12 border border-gold/20 px-1 py-0.5 text-xs text-brown text-center"
                        />
                        <button
                            onClick={() => addMutation.mutate(parseInt(qty) || 1)}
                            disabled={addMutation.isPending}
                            className="text-xs bg-brown text-cream px-2 py-1 hover:bg-brown-mid disabled:opacity-50"
                        >+</button>
                        <button onClick={() => setAdding(false)} className="text-xs text-brown-light px-1">✕</button>
                    </div>
                ) : (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => removeMutation.mutate(1)}
                            disabled={removeMutation.isPending || item.quantityAvailable === 0}
                            className="text-xs border border-gold/20 text-brown-light px-2 py-0.5 hover:border-gold/50 disabled:opacity-30"
                        >−</button>
                        <button
                            onClick={() => setAdding(true)}
                            className="text-xs border border-gold/20 text-brown-light px-2 py-0.5 hover:border-gold/50"
                        >+</button>
                    </div>
                )}
            </div>
        </div>
    )
}
