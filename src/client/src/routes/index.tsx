import { createFileRoute, Link } from '@tanstack/react-router'

const FALLBACK_IMAGES = [
  '/jean-pierre-brungs-3XoiSqiX5ms-unsplash.jpg',
  '/photo-1543960382-bdd97f0e7fdc.jpg',
  '/photo-1600705852854-402227d1662f.jpg',
  '/photo-1739918533428-040764352a53.jpg',
  '/photo-1765000884377-5134d3dc9d15.jpg',
]
import { useQuery } from '@tanstack/react-query'
import { inventoryApi } from '../lib/inventoryApi'
import type { ProductListDto } from '../lib/types'

export const Route = createFileRoute('/')({
  component: HomePage,
})

function HomePage() {
  const { data: products } = useQuery<ProductListDto[]>({
    queryKey: ['products'],
    queryFn: inventoryApi.getProducts,
  })

  const featured = products?.slice(0, 3) ?? []

  return (
    <div>
      {/* Hero */}
      <section className="relative overflow-hidden">
        <div className="max-w-7xl mx-auto px-10 py-16 lg:py-20">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
            {/* Left: text */}
            <div>
              <div className="inline-block border border-gold px-4 py-1.5 mb-8">
                <span className="text-[10px] tracking-[0.3em] uppercase text-gold font-semibold">Est. 2020</span>
              </div>
              <h1 className="mb-2">
                <span className="block font-serif text-5xl lg:text-6xl text-brown leading-tight">
                  Exquisite High Tea
                </span>
                <span className="block font-script text-5xl lg:text-6xl text-brown leading-snug">
                  for Every Occasion
                </span>
              </h1>
              <p className="text-brown-mid text-sm leading-relaxed mt-6 mb-10 max-w-sm">
                Curated collections of vintage china and elegant tea service sets to make your event truly unforgettable.
              </p>
              <div className="flex flex-wrap gap-4">
                <Link
                  to="/products"
                  className="inline-block bg-brown text-cream px-8 py-3.5 text-xs font-semibold tracking-[0.2em] uppercase hover:bg-brown-mid transition-colors"
                >
                  View Collection
                </Link>
                <Link
                  to="/availability"
                  className="inline-block border border-gold text-gold px-8 py-3.5 text-xs font-semibold tracking-[0.2em] uppercase hover:bg-gold/5 transition-colors"
                >
                  Book Now
                </Link>
              </div>
            </div>

            {/* Right: image placeholder */}
            <div className="relative flex justify-center lg:justify-end">
              <div className="relative w-full max-w-md lg:max-w-none aspect-4/3 border border-gold/60">
                <div className="absolute inset-3 bg-cream-dark overflow-hidden">
                  <img
                    src="/photo-1739918533428-040764352a53.jpg"
                    alt="High tea table setting"
                    className="w-full h-full object-cover object-top"
                  />
                </div>
                <span className="absolute top-0 left-0 w-6 h-6 border-t-2 border-l-2 border-gold" />
                <span className="absolute top-0 right-0 w-6 h-6 border-t-2 border-r-2 border-gold" />
                <span className="absolute bottom-0 left-0 w-6 h-6 border-b-2 border-l-2 border-gold" />
                <span className="absolute bottom-0 right-0 w-6 h-6 border-b-2 border-r-2 border-gold" />
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Why Choose Us */}
      <section className="bg-cream-dark py-24">
        <div className="max-w-5xl mx-auto px-6">
          <div className="text-center mb-14">
            <h2 className="font-serif text-4xl text-brown mb-3">Why Choose Us</h2>
            <div className="w-14 h-0.5 bg-gold mx-auto" />
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-12">
            {[
              {
                title: 'Curated with Love',
                desc: 'Each piece is hand-selected for its beauty, history, and charm. Our vintage china tells a story.',
                icon: (
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" className="w-6 h-6">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z" />
                  </svg>
                ),
              },
              {
                title: 'Themed Collections',
                desc: 'From vintage rose to art deco elegance, each set is thoughtfully themed to match your event\'s aesthetic.',
                icon: (
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" className="w-6 h-6">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M9.813 15.904L9 18.75l-.813-2.846a4.5 4.5 0 00-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 003.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 003.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 00-3.09 3.09zM18.259 8.715L18 9.75l-.259-1.035a3.375 3.375 0 00-2.455-2.456L14.25 6l1.036-.259a3.375 3.375 0 002.455-2.456L18 2.25l.259 1.035a3.375 3.375 0 002.456 2.456L21.75 6l-1.035.259a3.375 3.375 0 00-2.456 2.456z" />
                  </svg>
                ),
              },
              {
                title: 'Complete Service',
                desc: 'Everything you need for a perfect high tea — from teacups to cake stands and serving platters.',
                icon: (
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" className="w-6 h-6">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 8.25v-1.5m0 1.5c-1.355 0-2.697.056-4.024.166C6.845 8.51 6 9.473 6 10.608v2.513m6-4.871c1.355 0 2.697.056 4.024.166C17.155 8.51 18 9.473 18 10.608v2.513M15 8.25v-1.5A2.25 2.25 0 0012.75 4.5h-1.5A2.25 2.25 0 009 6.75v1.5M3 18a3 3 0 003 3h12a3 3 0 003-3v-4.5a.75.75 0 00-.75-.75H3.75a.75.75 0 00-.75.75V18z" />
                  </svg>
                ),
              },
            ].map(f => (
              <div key={f.title} className="text-center flex flex-col items-center">
                <div className="w-16 h-16 rounded-full border border-gold text-gold flex items-center justify-center mb-6">
                  {f.icon}
                </div>
                <h3 className="font-serif text-lg font-semibold text-brown mb-3">{f.title}</h3>
                <p className="text-sm text-brown-mid leading-relaxed max-w-xs">{f.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Featured Collections */}
      <section className="py-24">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center mb-14">
            <h2 className="font-serif text-4xl text-brown mb-3">Featured Collections</h2>
            <div className="w-14 h-0.5 bg-gold mx-auto mb-4" />
            <p className="text-brown-mid text-sm leading-relaxed">
              A glimpse of our exquisite vintage china sets
            </p>
          </div>

          {featured.length > 0 ? (
            <>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                {featured.map((product, i) => (
                  <Link
                    key={product.id}
                    to="/products"
                    className="relative group overflow-hidden aspect-4/3 block"
                  >
                    <img
                      src={product.featuredPhotoUrl ?? FALLBACK_IMAGES[i % FALLBACK_IMAGES.length]}
                      alt={product.name}
                      className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700"
                    />
                    {/* Dark gradient overlay */}
                    <div className="absolute inset-0 bg-linear-to-t from-brown/80 via-brown/20 to-transparent" />
                    {/* Text overlay */}
                    <div className="absolute bottom-0 left-0 right-0 p-5">
                      <p className="font-script text-3xl text-white leading-tight">{product.name}</p>
                      {product.colour && (
                        <p className="text-cream/70 text-xs mt-1 tracking-wide">{product.colour}</p>
                      )}
                    </div>
                  </Link>
                ))}
              </div>

              <div className="text-center mt-10">
                <Link
                  to="/products"
                  className="inline-block border border-gold text-brown px-10 py-3.5 text-xs font-semibold tracking-[0.2em] uppercase hover:bg-gold/5 transition-colors"
                >
                  Explore All Collections
                </Link>
              </div>
            </>
          ) : (
            /* Static cards with real images */
            <>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                {[
                  {
                    name: 'Rose Garden',
                    sub: 'Delicate florals & vintage charm',
                    img: '/jean-pierre-brungs-3XoiSqiX5ms-unsplash.jpg',
                    pos: 'object-center',
                  },
                  {
                    name: 'Golden Elegance',
                    sub: 'Sophisticated gold accents',
                    img: '/photo-1543960382-bdd97f0e7fdc.jpg',
                    pos: 'object-center',
                  },
                  {
                    name: 'Classic Romance',
                    sub: 'Timeless beauty & grace',
                    img: '/photo-1600705852854-402227d1662f.jpg',
                    pos: 'object-[center_30%]',
                  },
                ].map(c => (
                  <Link key={c.name} to="/products" className="relative aspect-4/3 bg-cream-dark overflow-hidden group block">
                    <img
                      src={c.img}
                      alt={c.name}
                      className={`absolute inset-0 w-full h-full object-cover ${c.pos} group-hover:scale-105 transition-transform duration-700`}
                    />
                    <div className="absolute inset-0 bg-linear-to-t from-brown/75 via-brown/10 to-transparent" />
                    <div className="absolute bottom-0 left-0 right-0 p-5">
                      <p className="font-script text-3xl text-white leading-tight">{c.name}</p>
                      <p className="text-cream/70 text-xs mt-1 tracking-wide">{c.sub}</p>
                    </div>
                  </Link>
                ))}
              </div>
              <div className="text-center mt-10">
                <Link
                  to="/products"
                  className="inline-block border border-gold text-brown px-10 py-3.5 text-xs font-semibold tracking-[0.2em] uppercase hover:bg-gold/5 transition-colors"
                >
                  Explore All Collections
                </Link>
              </div>
            </>
          )}
        </div>
      </section>

      {/* CTA Banner */}
      <section className="bg-brown py-20">
        <div className="max-w-3xl mx-auto px-6 text-center">
          <p className="font-script text-5xl text-gold-light mb-4">
            Ready to Create Magic?
          </p>
          <p className="text-cream/60 text-sm mb-10 tracking-wide leading-relaxed">
            Let us help you craft an unforgettable high tea experience with our beautiful vintage collections.
          </p>
          <Link
            to="/availability"
            className="inline-block bg-gold text-brown px-10 py-4 text-xs font-semibold tracking-[0.2em] uppercase hover:bg-gold-light transition-colors"
          >
            Start Your Booking
          </Link>
        </div>
      </section>
    </div>
  )
}
