export function Footer() {
  return (
    <footer className="bg-cream border-t border-gold/20 mt-auto">
      <div className="max-w-7xl mx-auto px-6 py-12">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {/* Brand */}
          <div>
            <p className="font-script text-3xl text-brown mb-1">Teacup Boutique</p>
            <p className="text-xs tracking-widest uppercase text-brown-light mb-3">High Tea Hire</p>
            <p className="text-sm text-brown-mid">Elegant high tea hire for your special occasions</p>
          </div>

          {/* Contact */}
          <div>
            <p className="text-xs tracking-widest uppercase text-brown-light font-semibold mb-4">Contact</p>
            <div className="space-y-2 text-sm text-brown-mid">
              <p>hello@teacupboutique.com</p>
              <p>(555) 123-4567</p>
            </div>
          </div>

          {/* Hours */}
          <div>
            <p className="text-xs tracking-widest uppercase text-brown-light font-semibold mb-4">Hours</p>
            <div className="space-y-2 text-sm text-brown-mid">
              <p>Monday – Friday: 9am – 5pm</p>
              <p>Weekend: By appointment</p>
            </div>
          </div>
        </div>

        <div className="border-t border-gold/20 mt-10 pt-6 text-center text-xs text-brown-light">
          © {new Date().getFullYear()} Teacup Boutique. All rights reserved.
        </div>
      </div>
    </footer>
  )
}
