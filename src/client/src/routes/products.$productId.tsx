import { createFileRoute, Link } from '@tanstack/react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'
import { inventoryApi } from '../lib/inventoryApi'
import { ProductForm } from '../components/ProductForm'
import { useAuth } from '../lib/useAuth'
import type { Photo, ProductDetail, ProductSet, ProductUpdateDto, SetItemCreateDto, SetItemDetail } from '../lib/types'

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
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['set-items', productId] })
            queryClient.invalidateQueries({ queryKey: ['product', productId] })
        },
    })

    const deactivateSetItemMutation = useMutation({
        mutationFn: (setItemId: string) => inventoryApi.deactivateSetItem(productId, setItemId, accessToken!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['set-items', productId] })
            queryClient.invalidateQueries({ queryKey: ['product', productId] })
            queryClient.invalidateQueries({ queryKey: ['products'] })
        },
    })

    const activateSetItemMutation = useMutation({
        mutationFn: (setItemId: string) => inventoryApi.activateSetItem(productId, setItemId, accessToken!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['set-items', productId] })
            queryClient.invalidateQueries({ queryKey: ['product', productId] })
            queryClient.invalidateQueries({ queryKey: ['products'] })
        },
    })

    const uploadPhotoMutation = useMutation({
        mutationFn: (file: File) => inventoryApi.uploadProductPhoto(productId, file, accessToken!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['product', productId] })
            queryClient.invalidateQueries({ queryKey: ['products'] })
        },
    })

    const deletePhotoMutation = useMutation({
        mutationFn: (photoId: string) => inventoryApi.deleteProductPhoto(productId, photoId, accessToken!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['product', productId] })
            queryClient.invalidateQueries({ queryKey: ['products'] })
        },
    })

    const setFeaturedMutation = useMutation({
        mutationFn: (photoId: string) => inventoryApi.setFeaturedPhoto(productId, photoId, accessToken!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['product', productId] })
            queryClient.invalidateQueries({ queryKey: ['products'] })
        },
    })

    const product = productQuery.data

    return (
        <div>
            {/* Back nav */}
            <div className="border-b border-gold/20 py-4 px-6 bg-white">
                <div className="max-w-6xl mx-auto">
                    <Link to="/products" className="text-xs tracking-[0.3em] uppercase text-gold font-semibold hover:text-gold/70 transition-colors">
                        ← Back to Products
                    </Link>
                </div>
            </div>

            <div className="max-w-6xl mx-auto px-6 py-12 space-y-8">
                {productQuery.isPending && <p className="text-brown-light font-serif text-lg text-center py-10">Loading…</p>}
                {productQuery.isError && <p className="text-red-600 text-sm">{productQuery.error.message}</p>}

                {product && (
                    <>
                        {/* Hero */}
                        <div className="flex flex-col md:flex-row bg-white overflow-hidden relative">
                            {/* Gold corner brackets */}
                            <span className="absolute top-0 left-0 w-8 h-8 border-t-2 border-l-2 border-gold z-10 pointer-events-none" />
                            <span className="absolute bottom-0 right-0 w-8 h-8 border-b-2 border-r-2 border-gold z-10 pointer-events-none" />

                            {/* Gallery */}
                            <div className="md:w-[45%] shrink-0 self-stretch min-h-96">
                                <ProductGallery photos={product.photos ?? []} name={product.name} />
                            </div>

                            {/* Details */}
                            <div className="flex-1 flex flex-col px-10 py-10">
                                {editingProduct ? (
                                    <>
                                        <div className="flex items-center justify-between mb-5">
                                            <h2 className="font-serif text-lg text-brown">Edit Product</h2>
                                            <button
                                                onClick={() => setEditingProduct(false)}
                                                className="text-xs text-gold hover:text-gold/70 transition-colors font-medium"
                                            >
                                                Cancel
                                            </button>
                                        </div>
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
                                    <>
                                        {isAdmin && (
                                            <span className={`inline-block self-start mb-4 text-xs tracking-widest uppercase px-3 py-1 ${product.isActive ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                                                {product.isActive ? 'Active' : 'Inactive'}
                                            </span>
                                        )}

                                        <h1 className="font-script text-5xl text-brown mb-2 leading-tight">{product.name}</h1>
                                        <div className="w-10 h-0.5 bg-gold mb-5" />

                                        {product.colour && (
                                            <span className="inline-block self-start mb-4 bg-brown/80 text-cream px-4 py-1 text-xs tracking-widest uppercase">
                                                {product.colour}
                                            </span>
                                        )}

                                        <p className="text-brown-mid text-sm leading-relaxed mb-6">{product.description}</p>

                                        {/* Stats row */}
                                        <div className="border-y border-gold/20 py-4 mb-6 flex justify-around">
                                            <div className="flex items-center gap-3">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" className="w-5 h-5 text-gold shrink-0">
                                                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 11-18 0 9 9 0 0118 0z" />
                                                </svg>
                                                <div>
                                                    <p className="text-xs text-gold tracking-widest uppercase mb-0.5">Price</p>
                                                    <p className="text-sm font-semibold text-brown">${product.price.toFixed(2)} <span className="font-normal text-brown-light text-xs">/ day</span></p>
                                                </div>
                                            </div>
                                            {product.servings > 0 && (
                                                <div className="flex items-center gap-3">
                                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" className="w-5 h-5 text-gold shrink-0">
                                                        <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0A17.933 17.933 0 0112 21.75c-2.676 0-5.216-.584-7.499-1.632z" />
                                                    </svg>
                                                    <div>
                                                        <p className="text-xs text-gold tracking-widest uppercase mb-0.5">Serves</p>
                                                        <p className="text-sm font-semibold text-brown">{product.servings} guests</p>
                                                    </div>
                                                </div>
                                            )}
                                            <div className="flex items-center gap-3">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" className="w-5 h-5 text-gold shrink-0">
                                                    <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 18.75a60.07 60.07 0 0115.797 2.101c.727.198 1.453-.342 1.453-1.096V18.75M3.75 4.5v.75A.75.75 0 013 6h-.75m0 0v-.375c0-.621.504-1.125 1.125-1.125H20.25M2.25 6v9m18-10.5v.75c0 .414.336.75.75.75h.75m-1.5-1.5h.375c.621 0 1.125.504 1.125 1.125v9.75c0 .621-.504 1.125-1.125 1.125h-.375m1.5-1.5H21a.75.75 0 00-.75.75v.75m0 0H3.75m0 0h-.375a1.125 1.125 0 01-1.125-1.125V15m1.5 1.5v-.75A.75.75 0 003 15h-.75M15 10.5a3 3 0 11-6 0 3 3 0 016 0zm3 0h.008v.008H18V10.5zm-12 0h.008v.008H6V10.5z" />
                                                </svg>
                                                <div>
                                                    <p className="text-xs text-gold tracking-widest uppercase mb-0.5">Deposit</p>
                                                    <p className="text-sm font-semibold text-brown">${product.depositAmount.toFixed(2)}</p>
                                                </div>
                                            </div>
                                        </div>

                                        {/* Contents */}
                                        {product.contents && (
                                            <div className="mb-8">
                                                <div className="flex items-center gap-2 mb-3">
                                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" className="w-4 h-4 text-gold">
                                                        <path strokeLinecap="round" strokeLinejoin="round" d="M9.813 15.904L9 18.75l-.813-2.846a4.5 4.5 0 00-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 003.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 003.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 00-3.09 3.09zM18.259 8.715L18 9.75l-.259-1.035a3.375 3.375 0 00-2.455-2.456L14.25 6l1.036-.259a3.375 3.375 0 002.455-2.456L18 2.25l.259 1.035a3.375 3.375 0 002.456 2.456L21.75 6l-1.035.259a3.375 3.375 0 00-2.456 2.456z" />
                                                    </svg>
                                                    <span className="text-xs tracking-[0.2em] uppercase text-gold font-semibold">Includes</span>
                                                </div>
                                                <p className="text-sm text-brown-mid leading-relaxed">
                                                    {product.contents.split('\n').filter(Boolean).map(item => item.trim()).join(', ')}
                                                </p>
                                            </div>
                                        )}

                                        {/* Admin rental details */}
                                        {isAdmin && (
                                            <div className="mb-6 grid grid-cols-3 gap-3 text-xs text-brown-light border border-gold/10 p-3">
                                                <div><span className="block text-gold uppercase tracking-widest mb-0.5">Min Days</span>{product.minRentalDays}</div>
                                                <div><span className="block text-gold uppercase tracking-widest mb-0.5">Max Days</span>{product.maxRentalDays}</div>
                                                <div><span className="block text-gold uppercase tracking-widest mb-0.5">Buffer</span>{product.bufferDays}</div>
                                            </div>
                                        )}

                                        {/* CTA */}
                                        <div className="mt-auto flex flex-col gap-3">
                                            <Link
                                                to="/availability"
                                                className="w-full bg-brown text-cream py-4 text-xs font-semibold tracking-[0.2em] uppercase hover:bg-brown-mid transition-colors text-center block"
                                            >
                                                Check Availability
                                            </Link>
                                            {isAdmin && (
                                                <button
                                                    onClick={() => setEditingProduct(true)}
                                                    className="text-xs text-gold hover:text-gold/70 transition-colors font-medium text-center"
                                                >
                                                    Edit Product
                                                </button>
                                            )}
                                        </div>
                                    </>
                                )}
                            </div>
                        </div>

                        {/* Admin-only inventory management */}
                        {isAdmin && (
                            <>
                                <PhotosSection
                                    photos={product.photos ?? []}
                                    uploadMutation={uploadPhotoMutation}
                                    deleteMutation={deletePhotoMutation}
                                    setFeaturedMutation={setFeaturedMutation}
                                />
                                <ComponentsSection
                                    items={setItemsQuery.data ?? []}
                                    isPending={setItemsQuery.isPending}
                                    addMutation={addSetItemMutation}
                                    deactivateMutation={deactivateSetItemMutation}
                                    activateMutation={activateSetItemMutation}
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

function ProductGallery({ photos, name }: { photos: Photo[]; name: string }) {
    const initialIndex = photos.findIndex(p => p.isFeatured)
    const [activeIndex, setActiveIndex] = useState(initialIndex >= 0 ? initialIndex : 0)
    const [lightboxOpen, setLightboxOpen] = useState(false)

    useEffect(() => {
        if (!lightboxOpen) return
        function handleKey(e: KeyboardEvent) {
            if (e.key === 'Escape') setLightboxOpen(false)
            else if (e.key === 'ArrowLeft') setActiveIndex(i => (i - 1 + photos.length) % photos.length)
            else if (e.key === 'ArrowRight') setActiveIndex(i => (i + 1) % photos.length)
        }
        window.addEventListener('keydown', handleKey)
        return () => window.removeEventListener('keydown', handleKey)
    }, [lightboxOpen, photos.length])

    if (photos.length === 0) {
        return (
            <div className="w-full h-full min-h-96 bg-cream-dark flex items-center justify-center">
                <span className="text-brown-light text-sm italic font-serif">No photos yet</span>
            </div>
        )
    }

    const current = photos[activeIndex] ?? photos[0]

    return (
        <>
            {/* Main image */}
            <div
                className="relative bg-cream-dark overflow-hidden cursor-zoom-in"
                style={{ minHeight: '400px' }}
                onClick={() => setLightboxOpen(true)}
            >
                <img
                    src={current.url}
                    alt={name}
                    className="absolute inset-0 w-full h-full object-cover object-center"
                />
                {photos.length > 1 && (
                    <div className="absolute bottom-3 right-3 bg-black/50 text-white text-xs px-2 py-1">
                        {activeIndex + 1} / {photos.length}
                    </div>
                )}
            </div>

            {/* Thumbnail strip */}
            {photos.length > 1 && (
                <div className="flex gap-1.5 p-2 bg-white border-t border-gold/10 overflow-x-auto">
                    {photos.map((photo, i) => (
                        <button
                            key={photo.id}
                            onClick={() => setActiveIndex(i)}
                            className={`shrink-0 w-16 h-16 overflow-hidden border-2 transition-colors ${i === activeIndex ? 'border-gold' : 'border-transparent opacity-60 hover:opacity-100'}`}
                        >
                            <img src={photo.url} alt="" className="w-full h-full object-cover" />
                        </button>
                    ))}
                </div>
            )}

            {/* Lightbox */}
            {lightboxOpen && (
                <div
                    className="fixed inset-0 z-50 bg-black/80 flex items-center justify-center p-4"
                    onClick={() => setLightboxOpen(false)}
                >
                    <div
                        className="relative max-w-3xl w-full"
                        onClick={e => e.stopPropagation()}
                    >
                        <div className="flex items-center justify-between px-4 py-2 bg-brown/90">
                            <span className="font-serif text-cream text-sm">{name}</span>
                            <div className="flex items-center gap-4">
                                {photos.length > 1 && (
                                    <span className="text-xs text-cream/70">{activeIndex + 1} / {photos.length}</span>
                                )}
                                <button onClick={() => setLightboxOpen(false)} className="text-cream/70 hover:text-cream text-lg leading-none">×</button>
                            </div>
                        </div>
                        <div className="relative bg-black aspect-4/3">
                            <img src={current.url} alt={name} className="absolute inset-0 w-full h-full object-contain" />
                            {photos.length > 1 && (
                                <>
                                    <button
                                        onClick={() => setActiveIndex(i => (i - 1 + photos.length) % photos.length)}
                                        className="absolute left-2 top-1/2 -translate-y-1/2 bg-black/50 hover:bg-black/70 text-white w-9 h-9 flex items-center justify-center text-lg transition-colors"
                                    >
                                        ‹
                                    </button>
                                    <button
                                        onClick={() => setActiveIndex(i => (i + 1) % photos.length)}
                                        className="absolute right-2 top-1/2 -translate-y-1/2 bg-black/50 hover:bg-black/70 text-white w-9 h-9 flex items-center justify-center text-lg transition-colors"
                                    >
                                        ›
                                    </button>
                                </>
                            )}
                        </div>
                        {photos.length > 1 && (
                            <div className="flex gap-1.5 p-2 bg-black overflow-x-auto">
                                {photos.map((photo, i) => (
                                    <button
                                        key={photo.id}
                                        onClick={() => setActiveIndex(i)}
                                        className={`shrink-0 w-14 h-14 overflow-hidden border-2 transition-colors ${i === activeIndex ? 'border-gold' : 'border-transparent'}`}
                                    >
                                        <img src={photo.url} alt="" className="w-full h-full object-cover" />
                                    </button>
                                ))}
                            </div>
                        )}
                    </div>
                </div>
            )}
        </>
    )
}

function PhotosSection({
    photos,
    uploadMutation,
    deleteMutation,
    setFeaturedMutation,
}: {
    photos: Photo[]
    uploadMutation: ReturnType<typeof useMutation<unknown, Error, File>>
    deleteMutation: ReturnType<typeof useMutation<unknown, Error, string>>
    setFeaturedMutation: ReturnType<typeof useMutation<unknown, Error, string>>
}) {
    const fileInputRef = useRef<HTMLInputElement>(null)

    function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
        const file = e.target.files?.[0]
        if (file) uploadMutation.mutate(file)
        e.target.value = ''
    }

    return (
        <div className="bg-white border border-gold/20 p-6">
            <div className="flex items-center justify-between mb-5">
                <h2 className="font-serif text-lg text-brown">Photos</h2>
                <button
                    onClick={() => fileInputRef.current?.click()}
                    disabled={uploadMutation.isPending}
                    className="text-xs text-gold hover:text-gold/70 transition-colors font-medium disabled:opacity-50"
                >
                    {uploadMutation.isPending ? 'Uploading…' : '+ Upload Photo'}
                </button>
                <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/jpeg,image/png,image/webp"
                    className="hidden"
                    onChange={handleFileChange}
                />
            </div>

            {uploadMutation.isError && (
                <p className="text-xs text-red-600 mb-3">{uploadMutation.error.message}</p>
            )}

            {photos.length === 0 && (
                <p className="text-sm text-brown-light italic">No photos yet.</p>
            )}

            {photos.length > 0 && (
                <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                    {photos.map(photo => (
                        <div key={photo.id} className="relative group border border-gold/10">
                            <img
                                src={photo.url}
                                alt=""
                                className="w-full aspect-square object-cover"
                            />
                            {photo.isFeatured && (
                                <span className="absolute top-2 left-2 text-xs bg-gold text-white px-2 py-0.5 tracking-widest uppercase">
                                    Primary
                                </span>
                            )}
                            <div className="flex gap-2 p-2 bg-white border-t border-gold/10">
                                {!photo.isFeatured && (
                                    <button
                                        onClick={() => setFeaturedMutation.mutate(photo.id)}
                                        disabled={setFeaturedMutation.isPending}
                                        className="text-xs text-gold hover:text-gold/70 transition-colors disabled:opacity-50"
                                    >
                                        Set Primary
                                    </button>
                                )}
                                <button
                                    onClick={() => deleteMutation.mutate(photo.id)}
                                    disabled={deleteMutation.isPending}
                                    className="text-xs text-red-400 hover:text-red-600 transition-colors disabled:opacity-50 ml-auto"
                                >
                                    Delete
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    )
}

function ComponentsSection({
    items,
    isPending,
    addMutation,
    deactivateMutation,
    activateMutation,
}: {
    items: SetItemDetail[]
    isPending: boolean
    addMutation: ReturnType<typeof useMutation<unknown, Error, SetItemCreateDto>>
    deactivateMutation: ReturnType<typeof useMutation<unknown, Error, string>>
    activateMutation: ReturnType<typeof useMutation<unknown, Error, string>>
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
                    <div className="grid grid-cols-4 gap-4 text-xs font-semibold text-brown-light uppercase tracking-widest pb-2 border-b border-gold/10">
                        <span>Name</span>
                        <span className="text-center">Quantity</span>
                        <span className="text-right">Deposit / Unit</span>
                        <span></span>
                    </div>
                    {items.map(item => (
                        <div key={item.id} className={`grid grid-cols-4 gap-4 py-3 border-b border-gold/10 last:border-0 text-sm items-center ${!item.isActive ? 'opacity-40' : ''}`}>
                            <span className="text-brown">{item.name}{!item.isActive && <span className="ml-2 text-xs text-brown-light">(inactive)</span>}</span>
                            <span className="text-brown text-center">{item.quantity}</span>
                            <span className="text-brown text-right">${item.depositValuePerUnit.toFixed(2)}</span>
                            <span className="text-right">
                                {item.isActive ? (
                                    <button
                                        onClick={() => deactivateMutation.mutate(item.id)}
                                        disabled={deactivateMutation.isPending}
                                        className="text-xs text-red-400 hover:text-red-600 transition-colors disabled:opacity-50"
                                    >
                                        Deactivate
                                    </button>
                                ) : (
                                    <button
                                        onClick={() => activateMutation.mutate(item.id)}
                                        disabled={activateMutation.isPending}
                                        className="text-xs text-gold hover:text-gold/70 transition-colors disabled:opacity-50"
                                    >
                                        Activate
                                    </button>
                                )}
                            </span>
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

