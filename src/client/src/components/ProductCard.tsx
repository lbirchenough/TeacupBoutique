import { Link } from '@tanstack/react-router'
import type { ProductListDto } from '../lib/types'
import { cartStore } from '../lib/cartStore'

interface Props {
    product: ProductListDto
}

export function ProductCard({ product }: Props) {
    function handleAddToCart(e: React.MouseEvent) {
        e.preventDefault()
        cartStore.addItem({
            productId: product.id,
            name: product.name,
            pricePerDay: product.price,
            quantity: 1,
            colour: product.colour,
            imageUrl: product.featuredPhotoUrl,
        })
    }

    return (
        <Link
            to="/products/$productId"
            params={{ productId: product.id }}
            className="block bg-white rounded-xl border border-gray-200 shadow-sm hover:shadow-md transition-shadow overflow-hidden"
        >
            <div className="h-40 bg-gray-100 flex items-center justify-center">
                {product.featuredPhotoUrl ? (
                    <img
                        src={product.featuredPhotoUrl}
                        alt={product.name}
                        className="h-full w-full object-cover"
                    />
                ) : (
                    <span className="text-gray-400 text-sm">No photo</span>
                )}
            </div>
            <div className="p-4">
                <h2 className="font-semibold text-gray-900 truncate">{product.name}</h2>
                <p className="text-sm text-gray-500 mt-1 line-clamp-2">{product.description}</p>
                <div className="mt-3 flex items-center justify-between">
                    <span className="text-sm font-medium text-gray-700">${product.price.toFixed(2)}/day</span>
                    {product.colour && (
                        <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
                            {product.colour}
                        </span>
                    )}
                </div>
                <button
                    onClick={handleAddToCart}
                    className="mt-3 w-full rounded-md bg-indigo-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-indigo-700"
                >
                    Add to Cart
                </button>
            </div>
        </Link>
    )
}
