import { createFileRoute } from '@tanstack/react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { inventoryApi } from '../lib/inventoryApi'
import { ProductCard } from '../components/ProductCard'
import { ProductForm } from '../components/ProductForm'
import type { ProductCreateDto, ProductListDto } from '../lib/types'

export const Route = createFileRoute('/products/')({
    component: ProductsPage,
})

function ProductsPage() {
    const queryClient = useQueryClient()
    const [showForm, setShowForm] = useState(false)

    const { isPending, isError, data, error } = useQuery<ProductListDto[]>({
        queryKey: ['products'],
        queryFn: inventoryApi.getProducts,
    })

    const createMutation = useMutation({
        mutationFn: (dto: ProductCreateDto) => inventoryApi.createProduct(dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['products'] })
            setShowForm(false)
        },
    })

    return (
        <div className="max-w-6xl mx-auto px-4 py-8">
            <div className="flex items-center justify-between mb-6">
                <h1 className="text-2xl font-bold text-gray-900">Products</h1>
                <button
                    onClick={() => setShowForm(s => !s)}
                    className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700"
                >
                    {showForm ? 'Cancel' : '+ Add Product'}
                </button>
            </div>

            {showForm && (
                <div className="mb-8 bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
                    <h2 className="text-lg font-semibold text-gray-800 mb-4">New Product</h2>
                    {createMutation.isError && (
                        <p className="text-sm text-red-600 mb-3">{createMutation.error.message}</p>
                    )}
                    <ProductForm
                        onSubmit={dto => createMutation.mutate(dto)}
                        onCancel={() => setShowForm(false)}
                        submitting={createMutation.isPending}
                    />
                </div>
            )}

            {isPending && <p className="text-gray-500">Loading…</p>}
            {isError && <p className="text-red-600">Error: {error.message}</p>}

            {data && data.length === 0 && (
                <p className="text-gray-500">No products yet. Add one above.</p>
            )}

            <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                {data?.map(product => (
                    <ProductCard key={product.id} product={product} />
                ))}
            </div>
        </div>
    )
}
