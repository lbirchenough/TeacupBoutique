import { createFileRoute, Link } from '@tanstack/react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { inventoryApi } from '../lib/inventoryApi'
import { ProductForm } from '../components/ProductForm'
import { useAuth } from '../lib/useAuth'
import type { Photo, ProductCreateDto, ProductListDto } from '../lib/types'

const FALLBACK_IMAGES = [
    '/jean-pierre-brungs-3XoiSqiX5ms-unsplash.jpg',
    '/photo-1543960382-bdd97f0e7fdc.jpg',
    '/photo-1600705852854-402227d1662f.jpg',
    '/photo-1739918533428-040764352a53.jpg',
    '/photo-1765000884377-5134d3dc9d15.jpg',
]

export const Route = createFileRoute('/products/')({
    component: ProductsPage,
})

function ProductsPage() {
    const queryClient = useQueryClient()
    const [showForm, setShowForm] = useState(false)
    const [lightboxProduct, setLightboxProduct] = useState<ProductListDto | null>(null)
    const { isAdmin, accessToken } = useAuth()

    const { isPending, isError, data, error } = useQuery<ProductListDto[]>({
        queryKey: ['products'],
        queryFn: inventoryApi.getProducts,
    })

    const createMutation = useMutation({
        mutationFn: (dto: ProductCreateDto) => inventoryApi.createProduct(dto, accessToken!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['products'] })
            setShowForm(false)
        },
    })

    return (
        <div>
            {/* Page header */}
            <div className="text-center pt-16 pb-10 px-6">
                <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Browse</p>
                <h1 className="font-script text-6xl text-brown">Our Collection</h1>
                <p className="text-brown-mid mt-4 text-sm max-w-md mx-auto leading-relaxed">
                    Beautifully curated hire sets for every occasion
                </p>
                <div className="flex items-center justify-center gap-3 mt-5">
                    <div className="h-px w-12 bg-gold/40" />
                    <div className="w-1.5 h-1.5 rounded-full bg-gold/60" />
                    <div className="h-px w-12 bg-gold/40" />
                </div>
            </div>

            <div className="max-w-6xl mx-auto px-6 pt-8 pb-14">
                {/* Admin controls */}
                {isAdmin && (
                    <div className="flex justify-end mb-10">
                        <button
                            onClick={() => setShowForm(s => !s)}
                            className="text-xs tracking-widest uppercase text-brown-mid border border-brown/20 px-4 py-2 hover:bg-cream-dark transition-colors"
                        >
                            {showForm ? 'Cancel' : '+ Add Product'}
                        </button>
                    </div>
                )}

                {showForm && (
                    <div className="mb-12 bg-white border border-gold/20 rounded-xl p-6 shadow-sm">
                        <h2 className="font-serif text-xl text-brown mb-5">New Product</h2>
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

                {isPending && (
                    <p className="text-center text-brown-light py-24 font-serif text-lg">Loading collection…</p>
                )}
                {isError && (
                    <p className="text-center text-red-600 py-12">Error: {error.message}</p>
                )}
                {data?.length === 0 && (
                    <p className="text-center text-brown-light py-24 font-serif text-lg">No products yet.</p>
                )}

                {/* Alternating offset cards */}
                <div className="space-y-16 py-4">
                    {data?.map((product, i) => (
                        <CollectionCard key={product.id} product={product} reverse={i % 2 !== 0} index={i} onImageClick={() => setLightboxProduct(product)} />
                    ))}
                </div>
            </div>

            {lightboxProduct && (
                <PhotoLightbox
                    product={lightboxProduct}
                    onClose={() => setLightboxProduct(null)}
                />
            )}
        </div>
    )
}

function CollectionCard({ product, reverse, index, onImageClick }: { product: ProductListDto; reverse: boolean; index: number; onImageClick: () => void }) {

    // Alternating horizontal offset — even cards push right, odd push left
    const offsetClass = reverse ? '-translate-x-6' : 'translate-x-6'

    // Corner bracket positions alternate with the card direction
    // reverse=false (image left): top-right + bottom-left brackets on the card
    // reverse=true (image right): top-left + bottom-right brackets on the card
    const cornerTopLeft = reverse
    const cornerBottomRight = reverse
    const cornerTopRight = !reverse
    const cornerBottomLeft = !reverse

    return (
        <Link
            to="/products/$productId"
            params={{ productId: product.id }}
            className={`flex flex-col md:flex-row ${reverse ? 'md:flex-row-reverse' : ''} bg-white overflow-hidden hover:shadow-xl transition-shadow group relative ${offsetClass}`}
        >
            {/* Gold corner brackets */}
            {cornerTopLeft && (
                <span className="absolute top-0 left-0 w-8 h-8 border-t-2 border-l-2 border-gold z-10 pointer-events-none" />
            )}
            {cornerTopRight && (
                <span className="absolute top-0 right-0 w-8 h-8 border-t-2 border-r-2 border-gold z-10 pointer-events-none" />
            )}
            {cornerBottomLeft && (
                <span className="absolute bottom-0 left-0 w-8 h-8 border-b-2 border-l-2 border-gold z-10 pointer-events-none" />
            )}
            {cornerBottomRight && (
                <span className="absolute bottom-0 right-0 w-8 h-8 border-b-2 border-r-2 border-gold z-10 pointer-events-none" />
            )}

            {/* Image — self-stretch so it fills the full card height */}
            <div
                className="md:w-[45%] self-stretch bg-cream-dark overflow-hidden shrink-0 relative min-h-72 cursor-zoom-in"
                onClick={e => { e.preventDefault(); onImageClick() }}
            >
                {/* Colour/style tag */}
                {product.colour && (
                    <div className="absolute top-4 left-1/2 -translate-x-1/2 z-10 bg-brown/80 text-cream px-4 py-1 text-xs tracking-widest uppercase">
                        {product.colour}
                    </div>
                )}
                {(product.photos?.length ?? 0) > 1 && (
                    <div className="absolute bottom-3 right-3 z-10 bg-black/50 text-white text-xs px-2 py-1 rounded">
                        {product.photos!.length} photos
                    </div>
                )}
                <img
                    src={product.featuredPhotoUrl ?? FALLBACK_IMAGES[index % FALLBACK_IMAGES.length]}
                    alt={product.name}
                    className="absolute inset-0 w-full h-full object-cover object-center group-hover:scale-105 transition-transform duration-700"
                />
            </div>

            {/* Content */}
            <div className="flex-1 flex flex-col px-10 py-10">
                {/* Name + underline */}
                <h2 className="font-script text-5xl text-brown mb-2 leading-tight">{product.name}</h2>
                <div className="w-10 h-0.5 bg-gold mb-5" />

                {/* Description */}
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
                </div>

                {/* Includes */}
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

                {/* CTA button — pushed to bottom */}
                <div className="mt-auto">
                    <Link
                        to="/availability"
                        onClick={e => e.stopPropagation()}
                        className="w-full bg-brown text-cream py-4 text-xs font-semibold tracking-[0.2em] uppercase hover:bg-brown-mid transition-colors block text-center"
                    >
                        Check Availability
                    </Link>
                </div>
            </div>
        </Link>
    )
}

function PhotoLightbox({ product, onClose }: { product: ProductListDto; onClose: () => void }) {
    const photos: Photo[] = product.photos?.length
        ? product.photos
        : product.featuredPhotoUrl
            ? [{ id: 'fallback', url: product.featuredPhotoUrl, isFeatured: true, displayOrder: 0 }]
            : []

    const initialIndex = photos.findIndex(p => p.isFeatured)
    const [index, setIndex] = useState(initialIndex >= 0 ? initialIndex : 0)

    const prev = () => setIndex(i => (i - 1 + photos.length) % photos.length)
    const next = () => setIndex(i => (i + 1) % photos.length)

    useEffect(() => {
        function handleKey(e: KeyboardEvent) {
            if (e.key === 'Escape') onClose()
            else if (e.key === 'ArrowLeft') prev()
            else if (e.key === 'ArrowRight') next()
        }
        window.addEventListener('keydown', handleKey)
        return () => window.removeEventListener('keydown', handleKey)
    }, [onClose])

    if (photos.length === 0) return null

    const current = photos[index]

    return (
        <div
            className="fixed inset-0 z-50 bg-black/80 flex flex-col items-center justify-center p-4"
            onClick={onClose}
        >
            <div
                className="relative max-w-3xl w-full bg-black flex flex-col"
                onClick={e => e.stopPropagation()}
            >
                {/* Header */}
                <div className="flex items-center justify-between px-4 py-2 bg-brown/90">
                    <span className="font-serif text-cream text-sm">{product.name}</span>
                    <div className="flex items-center gap-4">
                        {photos.length > 1 && (
                            <span className="text-xs text-cream/70">{index + 1} / {photos.length}</span>
                        )}
                        <button onClick={onClose} className="text-cream/70 hover:text-cream text-lg leading-none">×</button>
                    </div>
                </div>

                {/* Main image */}
                <div className="relative bg-black aspect-4/3">
                    <img
                        src={current.url}
                        alt={product.name}
                        className="absolute inset-0 w-full h-full object-contain"
                    />
                    {photos.length > 1 && (
                        <>
                            <button
                                onClick={prev}
                                className="absolute left-2 top-1/2 -translate-y-1/2 bg-black/50 hover:bg-black/70 text-white w-9 h-9 flex items-center justify-center text-lg transition-colors"
                            >
                                ‹
                            </button>
                            <button
                                onClick={next}
                                className="absolute right-2 top-1/2 -translate-y-1/2 bg-black/50 hover:bg-black/70 text-white w-9 h-9 flex items-center justify-center text-lg transition-colors"
                            >
                                ›
                            </button>
                        </>
                    )}
                </div>

                {/* Thumbnail strip */}
                {photos.length > 1 && (
                    <div className="flex gap-1.5 p-2 bg-black overflow-x-auto">
                        {photos.map((photo, i) => (
                            <button
                                key={photo.id}
                                onClick={() => setIndex(i)}
                                className={`shrink-0 w-14 h-14 overflow-hidden border-2 transition-colors ${i === index ? 'border-gold' : 'border-transparent'}`}
                            >
                                <img src={photo.url} alt="" className="w-full h-full object-cover" />
                            </button>
                        ))}
                    </div>
                )}
            </div>
        </div>
    )
}
