import { useState } from 'react'
import type { ProductCreateDto, ProductDetail } from '../lib/types'

interface Props {
    initial?: ProductDetail
    onSubmit: (dto: ProductCreateDto) => void
    onCancel: () => void
    submitting?: boolean
}

const defaults: ProductCreateDto = {
    name: '',
    description: '',
    contents: '',
    colour: '',
    price: 0,
    depositAmount: 0,
    minRentalDays: 1,
    maxRentalDays: 2,
    bufferDays: 1,
    isActive: true,
    servings: 0,
}

export function ProductForm({ initial, onSubmit, onCancel, submitting }: Props) {
    const [form, setForm] = useState<ProductCreateDto>(
        initial
            ? {
                  name: initial.name,
                  description: initial.description,
                  contents: initial.contents ?? '',
                  colour: initial.colour ?? '',
                  price: initial.price,
                  depositAmount: initial.depositAmount,
                  minRentalDays: initial.minRentalDays,
                  maxRentalDays: initial.maxRentalDays,
                  bufferDays: initial.bufferDays,
                  isActive: initial.isActive,
                  servings: initial.servings,
              }
            : defaults
    )

    type StringNumKey = 'name' | 'description' | 'contents' | 'colour' | 'servings' | 'price' | 'depositAmount' | 'minRentalDays' | 'maxRentalDays' | 'bufferDays'

    function field(key: StringNumKey) {
        return {
            value: (form[key] ?? '') as string | number,
            onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
                const value = e.target.type === 'number' ? Number(e.target.value) : e.target.value
                setForm(f => ({ ...f, [key]: value }))
            },
        }
    }

    return (
        <form
            onSubmit={e => {
                e.preventDefault()
                onSubmit(form)
            }}
            className="space-y-4"
        >
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div className="sm:col-span-2">
                    <label className="block text-sm font-medium text-gray-700">Name *</label>
                    <input
                        required
                        type="text"
                        {...field('name')}
                        className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                </div>
                <div className="sm:col-span-2">
                    <label className="block text-sm font-medium text-gray-700">Description *</label>
                    <textarea
                        required
                        rows={3}
                        {...field('description')}
                        className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                </div>
                <div className="sm:col-span-2">
                    <label className="block text-sm font-medium text-gray-700">Contents <span className="text-gray-400 font-normal">(one item per line)</span></label>
                    <textarea
                        rows={4}
                        placeholder={"Teacups\nSaucers\nTeapot\nCake stand"}
                        {...field('contents')}
                        className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Colour</label>
                    <input
                        type="text"
                        {...field('colour')}
                        className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Servings</label>
                    <input
                        type="number"
                        min={0}
                        {...field('servings')}
                        className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Price ($)</label>
                    <input
                        type="number"
                        min={0}
                        step="0.01"
                        {...field('price')}
                        className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Deposit ($)</label>
                    <input
                        type="number"
                        min={0}
                        step="0.01"
                        {...field('depositAmount')}
                        className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Min Rental Days</label>
                    <input
                        type="number"
                        min={1}
                        {...field('minRentalDays')}
                        className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Max Rental Days</label>
                    <input
                        type="number"
                        min={1}
                        {...field('maxRentalDays')}
                        className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Buffer Days</label>
                    <input
                        type="number"
                        min={0}
                        {...field('bufferDays')}
                        className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                </div>
                <div className="flex items-center gap-2 pt-5">
                    <input
                        id="isActive"
                        type="checkbox"
                        checked={form.isActive}
                        onChange={e => setForm(f => ({ ...f, isActive: e.target.checked }))}
                        className="h-4 w-4 rounded border-gray-300 text-indigo-600"
                    />
                    <label htmlFor="isActive" className="text-sm font-medium text-gray-700">Active</label>
                </div>
            </div>

            <div className="flex gap-3 pt-2">
                <button
                    type="submit"
                    disabled={submitting}
                    className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50"
                >
                    {submitting ? 'Saving…' : 'Save'}
                </button>
                <button
                    type="button"
                    onClick={onCancel}
                    className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
                >
                    Cancel
                </button>
            </div>
        </form>
    )
}
