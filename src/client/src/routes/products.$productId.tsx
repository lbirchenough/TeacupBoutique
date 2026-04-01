import { createFileRoute, Link } from '@tanstack/react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { inventoryApi } from '../lib/inventoryApi'
import { ProductForm } from '../components/ProductForm'
import { cartStore } from '../lib/cartStore'
import { useAuth } from '../lib/useAuth'
import type { ProductDetail, ProductSet, ProductUpdateDto, SetItemCreateDto, SetItemDetail } from '../lib/types'

export const Route = createFileRoute('/products/$productId')({
    component: ProductDetailPage,
})

function ProductDetailPage() {
    const { productId } = Route.useParams()
    const queryClient = useQueryClient()
    const [editingProduct, setEditingProduct] = useState(false)
    const { isAdmin, accessToken } = useAuth()

    const productQuery = useQuery<ProductDetail>({
        queryKey: ['product', productId],
        queryFn: () => inventoryApi.getProduct(productId),
    })

    const setsQuery = useQuery<ProductSet[]>({
        queryKey: ['product-sets', productId],
        queryFn: () => inventoryApi.getProductSets(productId),
        enabled: isAdmin,
    })

    const setItemsQuery = useQuery<SetItemDetail[]>({
        queryKey: ['set-items', productId],
        queryFn: () => inventoryApi.getSetItems(productId),
        enabled: isAdmin,
    })

    const updateProductMutation = useMutation({
        mutationFn: (dto: ProductUpdateDto) => inventoryApi.updateProduct(productId, dto, accessToken!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['product', productId] })
            queryClient.invalidateQueries({ queryKey: ['products'] })
            setEditingProduct(false)
        },
    })

    const addSetMutation = useMutation({
        mutationFn: () => inventoryApi.createProductSet(productId, accessToken!),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['product-sets', productId] }),
    })

    const addSetItemMutation = useMutation({
        mutationFn: (dto: SetItemCreateDto) => inventoryApi.createSetItem(productId, dto, accessToken!),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['set-items', productId] }),
    })

    const product = productQuery.data

    return (
        <div>
            <div className="bg-cream-dark border-b border-gold/20 py-16 text-center">
                <Link to="/products" className="text-xs tracking-[0.3em] uppercase text-gold font-semibold hover:text-gold/70 transition-colors">
                    ← Back to Products
                </Link>
                {product && (
                    <>
                        <h1 className="font-serif text-3xl text-brown mt-4">{product.name}</h1>
                        <span className={`inline-block mt-2 text-xs tracking-widest uppercase px-3 py-1 ${product.isActive ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                            {product.isActive ? 'Active' : 'Inactive'}
                        </span>
                    </>
                )}
            </div>

            <div className="max-w-4xl mx-auto px-6 py-12 space-y-8">
                {productQuery.isPending && <p className="text-brown-light font-serif text-lg text-center py-10">Loading…</p>}
                {productQuery.isError && <p className="text-red-600 text-sm">{productQuery.error.message}</p>}

                {product && (
                    <>
                        {/* Product details */}
                        <div className="bg-white border border-gold/20 p-6">
                            <div className="flex items-start justify-between mb-5">
                                <h2 className="font-serif text-lg text-brown">Product Details</h2>
                                <div className="flex gap-4">
                                    <button
                                        onClick={() => cartStore.addItem({
                                            productId: product.id,
                                            name: product.name,
                                            pricePerDay: product.price,
                                            depositAmount: product.depositAmount,
                                            quantity: 1,
                                            servings: product.servings,
                                            colour: product.colour,
                                            imageUrl: product.photos?.find(p => p.isFeatured)?.url,
                                        })}
                                        className="text-xs bg-brown text-cream px-4 py-2 hover:bg-brown-mid transition-colors"
                                    >
                                        Add to Cart
                                    </button>
                                    {isAdmin && (
                                        <button
                                            onClick={() => setEditingProduct(e => !e)}
                                            className="text-xs text-gold hover:text-gold/70 transition-colors font-medium"
                                        >
                                            {editingProduct ? 'Cancel' : 'Edit'}
                                        </button>
                                    )}
                                </div>
                            </div>

                            {editingProduct ? (
                                <>
                                    {updateProductMutation.isError && (
                                        <p className="text-sm text-red-600 mb-3">{updateProductMutation.error.message}</p>
                                    )}
                                    <ProductForm
                                        initial={product}
                                        onSubmit={dto => updateProductMutation.mutate(dto)}
                                        onCancel={() => setEditingProduct(false)}
                                        submitting={updateProductMutation.isPending}
                                    />
                                </>
                            ) : (
                                <div className="grid grid-cols-2 gap-x-8 gap-y-3 text-sm">
                                    <DetailRow label="Description" value={product.description} span />
                                    {product.contents && <DetailRow label="Contents" value={product.contents} span />}
                                    {product.colour && <DetailRow label="Colour" value={product.colour} />}
                                    <DetailRow label="Servings" value={product.servings} />
                                    <DetailRow label="Price" value={`$${product.price.toFixed(2)}`} />
                                    <DetailRow label="Deposit" value={`$${product.depositAmount.toFixed(2)}`} />
                                    <DetailRow label="Min Rental Days" value={product.minRentalDays} />
                                    <DetailRow label="Max Rental Days" value={product.maxRentalDays} />
                                    <DetailRow label="Buffer Days" value={product.bufferDays} />
                                </div>
                            )}
                        </div>

                        {/* Admin-only inventory management */}
                        {isAdmin && (
                            <>
                                <ComponentsSection
                                    items={setItemsQuery.data ?? []}
                                    isPending={setItemsQuery.isPending}
                                    addMutation={addSetItemMutation}
                                />
                                <PhysicalSetsSection
                                    sets={setsQuery.data ?? []}
                                    isPending={setsQuery.isPending}
                                    addMutation={addSetMutation}
                                />
                            </>
                        )}
                    </>
                )}
            </div>
        </div>
    )
}

function ComponentsSection({
    items,
    isPending,
    addMutation,
}: {
    items: SetItemDetail[]
    isPending: boolean
    addMutation: ReturnType<typeof useMutation<unknown, Error, SetItemCreateDto>>
}) {
    const [showForm, setShowForm] = useState(false)
    const [form, setForm] = useState({ name: '', quantity: '1', depositValuePerUnit: '0' })

    function handleSave() {
        const qty = parseInt(form.quantity)
        const deposit = parseFloat(form.depositValuePerUnit)
        if (!form.name.trim() || isNaN(qty) || qty < 1 || isNaN(deposit) || deposit < 0) return
        addMutation.mutate(
            { name: form.name.trim(), quantity: qty, depositValuePerUnit: deposit },
            {
                onSuccess: () => {
                    setShowForm(false)
                    setForm({ name: '', quantity: '1', depositValuePerUnit: '0' })
                },
            }
        )
    }

    return (
        <div className="bg-white border border-gold/20 p-6">
            <div className="flex items-center justify-between mb-5">
                <h2 className="font-serif text-lg text-brown">Components</h2>
                {!showForm && (
                    <button
                        onClick={() => setShowForm(true)}
                        className="text-xs text-gold hover:text-gold/70 transition-colors font-medium"
                    >
                        + Add Component
                    </button>
                )}
            </div>

            {isPending && <p className="text-sm text-brown-light">Loading…</p>}

            {!isPending && items.length === 0 && !showForm && (
                <p className="text-sm text-brown-light italic">No components defined yet.</p>
            )}

            {items.length > 0 && (
                <div className="mb-4">
                    <div className="grid grid-cols-3 gap-4 text-xs font-semibold text-brown-light uppercase tracking-widest pb-2 border-b border-gold/10">
                        <span>Name</span>
                        <span className="text-center">Quantity</span>
                        <span className="text-right">Deposit / Unit</span>
                    </div>
                    {items.map(item => (
                        <div key={item.id} className="grid grid-cols-3 gap-4 py-3 border-b border-gold/10 last:border-0 text-sm">
                            <span className="text-brown">{item.name}</span>
                            <span className="text-brown text-center">{item.quantity}</span>
                            <span className="text-brown text-right">${item.depositValuePerUnit.toFixed(2)}</span>
                        </div>
                    ))}
                </div>
            )}

            {showForm && (
                <div className="border border-gold/20 p-4 space-y-3">
                    <p className="text-xs font-semibold text-brown-light uppercase tracking-widest">New Component</p>
                    <div className="grid grid-cols-3 gap-3">
                        <div className="col-span-3 sm:col-span-1">
                            <label className="block text-xs text-brown-light mb-1">Name</label>
                            <input
                                type="text"
                                value={form.name}
                                onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                                placeholder="e.g. Teacup"
                                className="w-full border border-gold/20 px-3 py-1.5 text-sm text-brown focus:outline-none focus:border-gold/50"
                            />
                        </div>
                        <div>
                            <label className="block text-xs text-brown-light mb-1">Quantity</label>
                            <input
                                type="number"
                                min="1"
                                value={form.quantity}
                                onChange={e => setForm(f => ({ ...f, quantity: e.target.value }))}
                                className="w-full border border-gold/20 px-3 py-1.5 text-sm text-brown focus:outline-none focus:border-gold/50"
                            />
                        </div>
                        <div>
                            <label className="block text-xs text-brown-light mb-1">Deposit / Unit ($)</label>
                            <input
                                type="number"
                                min="0"
                                step="0.01"
                                value={form.depositValuePerUnit}
                                onChange={e => setForm(f => ({ ...f, depositValuePerUnit: e.target.value }))}
                                className="w-full border border-gold/20 px-3 py-1.5 text-sm text-brown focus:outline-none focus:border-gold/50"
                            />
                        </div>
                    </div>
                    {addMutation.isError && (
                        <p className="text-xs text-red-600">{addMutation.error.message}</p>
                    )}
                    <div className="flex gap-2">
                        <button
                            onClick={handleSave}
                            disabled={addMutation.isPending}
                            className="text-xs bg-brown text-cream px-4 py-1.5 hover:bg-brown-mid transition-colors disabled:opacity-50"
                        >
                            {addMutation.isPending ? 'Saving…' : 'Save'}
                        </button>
                        <button
                            onClick={() => { setShowForm(false); setForm({ name: '', quantity: '1', depositValuePerUnit: '0' }) }}
                            className="text-xs border border-gold/20 text-brown-light px-4 py-1.5 hover:border-gold/50 transition-colors"
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}
        </div>
    )
}

function PhysicalSetsSection({
    sets,
    isPending,
    addMutation,
}: {
    sets: ProductSet[]
    isPending: boolean
    addMutation: ReturnType<typeof useMutation<unknown, Error, void>>
}) {
    const statusBadge: Record<string, string> = {
        Available: 'bg-green-50 text-green-700',
        Maintenance: 'bg-amber-50 text-amber-700',
        Retired: 'bg-gray-100 text-gray-500',
    }

    return (
        <div className="bg-white border border-gold/20 p-6">
            <div className="flex items-center justify-between mb-5">
                <h2 className="font-serif text-lg text-brown">Physical Sets</h2>
                <button
                    onClick={() => addMutation.mutate()}
                    disabled={addMutation.isPending}
                    className="text-xs text-gold hover:text-gold/70 transition-colors font-medium disabled:opacity-50"
                >
                    {addMutation.isPending ? 'Adding…' : '+ Add Set'}
                </button>
            </div>

            {isPending && <p className="text-sm text-brown-light">Loading…</p>}

            {!isPending && sets.length === 0 && (
                <p className="text-sm text-brown-light italic">No sets yet.</p>
            )}

            {sets.length > 0 && (
                <div>
                    {sets.map(set => (
                        <div key={set.id} className="flex items-center justify-between py-3 border-b border-gold/10 last:border-0">
                            <span className="text-sm text-brown">{set.name}</span>
                            <span className={`text-xs px-2 py-1 ${statusBadge[set.status] ?? 'bg-gray-100 text-gray-500'}`}>
                                {set.status}
                            </span>
                        </div>
                    ))}
                </div>
            )}

            {addMutation.isError && (
                <p className="text-xs text-red-600 mt-2">{addMutation.error.message}</p>
            )}
        </div>
    )
}

function DetailRow({ label, value, span }: { label: string; value: string | number; span?: boolean }) {
    return (
        <div className={span ? 'col-span-2' : ''}>
            <span className="text-brown-light">{label}: </span>
            <span className="text-brown font-medium">{value}</span>
        </div>
    )
}
