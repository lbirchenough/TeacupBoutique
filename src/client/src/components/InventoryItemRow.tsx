import { useState } from 'react'
import type { Condition, InventoryItem, InventoryItemUpdateDto, ItemStatus } from '../lib/types'

interface Props {
    productId: string
    item: InventoryItem
    isAdmin: boolean
    onUpdate: (itemId: string, dto: InventoryItemUpdateDto) => Promise<void>
    onDelete: (itemId: string) => Promise<void>
}

const conditionColours: Record<Condition, string> = {
    New: 'bg-green-100 text-green-700',
    Good: 'bg-blue-100 text-blue-700',
    Fair: 'bg-yellow-100 text-yellow-700',
    Poor: 'bg-red-100 text-red-700',
}

const statusColours: Record<ItemStatus, string> = {
    Available: 'bg-emerald-100 text-emerald-700',
    Maintenance: 'bg-orange-100 text-orange-700',
    Retired: 'bg-gray-200 text-gray-500',
}

const rowBorder: Record<ItemStatus, string> = {
    Available: 'border-emerald-200',
    Maintenance: 'border-orange-200',
    Retired: 'border-gray-300',
}

export function InventoryItemRow({ item, isAdmin, onUpdate, onDelete }: Props) {
    const [expanded, setExpanded] = useState(false)
    const [editing, setEditing] = useState(false)
    const [saving, setSaving] = useState(false)
    const [deleting, setDeleting] = useState(false)
    const [form, setForm] = useState<InventoryItemUpdateDto>({
        id: item.id,
        condition: item.condition,
        status: item.status,
        conditionNotes: item.conditionNotes ?? '',
        maintenanceHistory: item.maintenanceHistory ?? '',
    })

    async function handleSave() {
        setSaving(true)
        try {
            await onUpdate(item.id, form)
            setEditing(false)
        } finally {
            setSaving(false)
        }
    }

    async function handleDelete() {
        if (!confirm('Delete this inventory item?')) return
        setDeleting(true)
        try {
            await onDelete(item.id)
        } finally {
            setDeleting(false)
        }
    }

    function handleRowClick() {
        if (!expanded) setExpanded(true)
        else if (!editing) setExpanded(false)
    }

    return (
        <div className={`border rounded-lg overflow-hidden ${rowBorder[item.status]}`}>
            {/* Header row — always visible, click to expand/collapse */}
            <div
                className="flex items-center justify-between px-4 py-3 bg-white cursor-pointer select-none"
                onClick={handleRowClick}
            >
                <div className="flex items-center gap-3">
                    <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${conditionColours[item.condition]}`}>
                        {item.condition}
                    </span>
                    <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${statusColours[item.status]}`}>
                        {item.status}
                    </span>
                    {!expanded && item.conditionNotes && (
                        <span className="text-xs text-gray-400 truncate max-w-xs">{item.conditionNotes}</span>
                    )}
                </div>
                <div className="flex items-center gap-3 ml-4 shrink-0">
                    {isAdmin && expanded && !editing && (
                        <button
                            onClick={e => { e.stopPropagation(); setEditing(true) }}
                            className="text-xs text-indigo-600 hover:text-indigo-800 font-medium"
                        >
                            Edit
                        </button>
                    )}
                    {isAdmin && (
                        <button
                            onClick={e => { e.stopPropagation(); handleDelete() }}
                            disabled={deleting}
                            className="text-xs text-red-500 hover:text-red-700 font-medium disabled:opacity-50"
                        >
                            {deleting ? '…' : 'Delete'}
                        </button>
                    )}
                    <span className="text-gray-400 text-xs">{expanded ? '▲' : '▼'}</span>
                </div>
            </div>

            {/* Expanded panel */}
            {expanded && (
                <div className="border-t border-gray-100 px-4 py-4 bg-gray-50">
                    {editing ? (
                        <div className="space-y-3">
                            <div className="grid grid-cols-2 gap-3">
                                <div>
                                    <label className="block text-xs font-medium text-gray-600 mb-1">Condition</label>
                                    <select
                                        value={form.condition}
                                        onChange={e => setForm(f => ({ ...f, condition: e.target.value as Condition }))}
                                        className="w-full rounded border border-gray-300 px-2 py-1.5 text-sm"
                                    >
                                        {(['New', 'Good', 'Fair', 'Poor'] as Condition[]).map(c => (
                                            <option key={c} value={c}>{c}</option>
                                        ))}
                                    </select>
                                </div>
                                <div>
                                    <label className="block text-xs font-medium text-gray-600 mb-1">Status</label>
                                    <select
                                        value={form.status}
                                        onChange={e => setForm(f => ({ ...f, status: e.target.value as ItemStatus }))}
                                        className="w-full rounded border border-gray-300 px-2 py-1.5 text-sm"
                                    >
                                        {(['Available', 'Maintenance', 'Retired'] as ItemStatus[]).map(s => (
                                            <option key={s} value={s}>{s}</option>
                                        ))}
                                    </select>
                                </div>
                                <div className="col-span-2">
                                    <label className="block text-xs font-medium text-gray-600 mb-1">Condition Notes</label>
                                    <input
                                        type="text"
                                        value={form.conditionNotes}
                                        onChange={e => setForm(f => ({ ...f, conditionNotes: e.target.value }))}
                                        className="w-full rounded border border-gray-300 px-2 py-1.5 text-sm"
                                    />
                                </div>
                                <div className="col-span-2">
                                    <label className="block text-xs font-medium text-gray-600 mb-1">Maintenance History</label>
                                    <textarea
                                        rows={2}
                                        value={form.maintenanceHistory}
                                        onChange={e => setForm(f => ({ ...f, maintenanceHistory: e.target.value }))}
                                        className="w-full rounded border border-gray-300 px-2 py-1.5 text-sm"
                                    />
                                </div>
                            </div>
                            <div className="flex gap-2">
                                <button
                                    onClick={handleSave}
                                    disabled={saving}
                                    className="rounded bg-indigo-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-indigo-700 disabled:opacity-50"
                                >
                                    {saving ? 'Saving…' : 'Save'}
                                </button>
                                <button
                                    onClick={() => setEditing(false)}
                                    className="rounded border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 hover:bg-gray-100"
                                >
                                    Cancel
                                </button>
                            </div>
                        </div>
                    ) : (
                        <dl className="grid grid-cols-2 gap-x-6 gap-y-2 text-sm">
                            <div>
                                <dt className="text-gray-500 text-xs">Condition Notes</dt>
                                <dd className="text-gray-800">{item.conditionNotes || <span className="text-gray-400 italic">None</span>}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500 text-xs">Maintenance History</dt>
                                <dd className="text-gray-800">{item.maintenanceHistory || <span className="text-gray-400 italic">None</span>}</dd>
                            </div>
                        </dl>
                    )}
                </div>
            )}
        </div>
    )
}
