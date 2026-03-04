import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/')({
  component: HomePage,
})

function HomePage() {
  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="text-center">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">
          Welcome to High Tea Rentals
        </h1>
        <p className="text-xl text-gray-600 mb-8">
          Rent elegant high tea cups, cutlery, and equipment for your special occasions
        </p>
        <div className="mt-12 grid grid-cols-1 md:grid-cols-3 gap-8">
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h2 className="text-2xl font-semibold mb-2">Elegant China</h2>
            <p className="text-gray-600">
              Beautiful tea cups and saucers for your high tea experience
            </p>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h2 className="text-2xl font-semibold mb-2">Fine Cutlery</h2>
            <p className="text-gray-600">
              Premium cutlery sets to complement your table setting
            </p>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h2 className="text-2xl font-semibold mb-2">Complete Sets</h2>
            <p className="text-gray-600">
              Everything you need for a perfect high tea gathering
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}

