import { createFileRoute, Link } from '@tanstack/react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { inventoryApi } from '../lib/inventoryApi'
import { ProductForm } from '../components/ProductForm'
import { InventoryItemRow } from '../components/InventoryItemRow'
import { cartStore } from '../lib/cartStore'
import type { InventoryItem, InventoryItemUpdateDto, ProductDetail, ProductUpdateDto } from '../lib/types'

export const Route = createFileRoute('/products/$productId')({
    component: ProductDetailPage,
})

function ProductDetailPage() {
    const { productId } = Route.useParams()
    const queryClient = useQueryClient()
    const [editingProduct, setEditingProduct] = useState(false)

    const productQuery = useQuery<ProductDetail>({
        queryKey: ['product', productId],
        queryFn: () => inventoryApi.getProduct(productId),
    })

    const itemsQuery = useQuery<InventoryItem[]>({
        queryKey: ['product-items', productId],
        queryFn: () => inventoryApi.getProductItems(productId),
    })

    const updateProductMutation = useMutation({
        mutationFn: (dto: ProductUpdateDto) => inventoryApi.updateProduct(productId, dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['product', productId] })
            queryClient.invalidateQueries({ queryKey: ['products'] })
            setEditingProduct(false)
        },
    })

    const addItemMutation = useMutation({
        mutationFn: () => inventoryApi.createInventoryItem(productId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['product-items', productId] })
        },
    })

    const updateItemMutation = useMutation({
        mutationFn: ({ itemId, dto }: { itemId: string; dto: InventoryItemUpdateDto }) =>
            inventoryApi.updateInventoryItem(productId, itemId, dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['product-items', productId] })
        },
    })

    const deleteItemMutation = useMutation({
        mutationFn: (itemId: string) => inventoryApi.deleteInventoryItem(productId, itemId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['product-items', productId] })
        },
    })

    const product = productQuery.data

    return (
        <div className="max-w-4xl mx-auto px-4 py-8">
            <Link to="/products" className="text-sm text-indigo-600 hover:text-indigo-800 mb-6 inline-block">
                ← Back to Products
            </Link>

            {productQuery.isPending && <p className="text-gray-500">Loading…</p>}
            {productQuery.isError && <p className="text-red-600">Error: {productQuery.error.message}</p>}

            {product && (
                <>
                    {/* Product details section */}
                    <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm mb-8">
                        <div className="flex items-start justify-between mb-4">
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900">{product.name}</h1>
                                <span className={`inline-block mt-1 text-xs px-2 py-0.5 rounded-full font-medium ${product.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                                    {product.isActive ? 'Active' : 'Inactive'}
                                </span>
                            </div>
                            <div className="flex gap-3">
                                <button
                                    onClick={() => cartStore.addItem({
                                        productId: product.id,
                                        name: product.name,
                                        pricePerDay: product.price,
                                        quantity: 1,
                                        colour: product.colour,
                                        imageUrl: product.photos?.find(p => p.isFeatured)?.url,
                                    })}
                                    className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-700"
                                >
                                    Add to Cart
                                </button>
                                <button
                                    onClick={() => setEditingProduct(e => !e)}
                                    className="text-sm text-indigo-600 hover:text-indigo-800 font-medium"
                                >
                                    {editingProduct ? 'Cancel' : 'Edit'}
                                </button>
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

                    {/* Inventory items section */}
                    <div>
                        <div className="flex items-center justify-between mb-4">
                            <h2 className="text-lg font-semibold text-gray-800">
                                Inventory Items
                                {itemsQuery.data && (
                                    <span className="ml-2 text-sm font-normal text-gray-500">
                                        ({itemsQuery.data.length})
                                    </span>
                                )}
                            </h2>
                            <button
                                onClick={() => addItemMutation.mutate()}
                                disabled={addItemMutation.isPending}
                                className="rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50"
                            >
                                {addItemMutation.isPending ? 'Adding…' : '+ Add Item'}
                            </button>
                        </div>

                        {itemsQuery.isPending && <p className="text-gray-500 text-sm">Loading items…</p>}
                        {itemsQuery.isError && <p className="text-red-600 text-sm">Error: {itemsQuery.error.message}</p>}

                        {itemsQuery.data && itemsQuery.data.length === 0 && (
                            <p className="text-gray-500 text-sm">No inventory items yet.</p>
                        )}

                        <div className="space-y-2">
                            {itemsQuery.data?.map(item => (
                                <InventoryItemRow
                                    key={item.id}
                                    productId={productId}
                                    item={item}
                                    onUpdate={(itemId, dto) => updateItemMutation.mutateAsync({ itemId, dto })}
                                    onDelete={itemId => deleteItemMutation.mutateAsync(itemId)}
                                />
                            ))}
                        </div>
                    </div>
                </>
            )}
        </div>
    )
}

function DetailRow({ label, value, span }: { label: string; value: string | number; span?: boolean }) {
    return (
        <div className={span ? 'col-span-2' : ''}>
            <span className="text-gray-500">{label}: </span>
            <span className="text-gray-900 font-medium">{value}</span>
        </div>
    )
}
