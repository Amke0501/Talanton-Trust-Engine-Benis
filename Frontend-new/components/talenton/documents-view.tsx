'use client'

import { useState, useMemo } from 'react'
import { FileText, Upload, CheckCircle2, Clock, Trash2, ShieldCheck, Download, AlertCircle, Sparkles } from 'lucide-react'
import { type Application, type DocumentSlot } from '@/lib/talenton-data'
import { uploadDocumentToStorage } from '@/lib/api-service'

export function DocumentsView({
  application,
  onUpdateDocuments,
}: {
  application: Application
  onUpdateDocuments?: (docs: DocumentSlot[]) => void
}) {
  const [selectedSlot, setSelectedSlot] = useState<string>('id')
  const [dragActive, setDragActive] = useState(false)
  const [isUploading, setIsUploading] = useState(false)

  const documentSlots = useMemo(() => {
    return application.documents || []
  }, [application])

  const pendingSlots = useMemo(() => {
    return documentSlots.filter(s => s.status !== 'VERIFIED' && !s.fileName)
  }, [documentSlots])

  // Handle drag events
  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    if (e.type === "dragenter" || e.type === "dragover") {
      setDragActive(true)
    } else if (e.type === "dragleave") {
      setDragActive(false)
    }
  }

  // Handle drop
  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setDragActive(false)
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileUpload(e.dataTransfer.files[0])
    }
  }

  // Handle file select
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      handleFileUpload(e.target.files[0])
    }
  }

  const handleFileUpload = async (file: File) => {
    const slot = documentSlots.find(s => s.id === selectedSlot)
    if (!slot) return

    setIsUploading(true)
    const fileUrl = await uploadDocumentToStorage(file, application.reference, selectedSlot)
    setIsUploading(false)

    const updatedSlots = documentSlots.map(s => {
      if (s.id === selectedSlot) {
        return { 
          ...s, 
          status: 'VERIFIED' as const, 
          fileName: file.name,
          fileUrl,
        }
      }
      return s
    })

    if (onUpdateDocuments) {
      onUpdateDocuments(updatedSlots)
    }
  }

  const handleDelete = (slotId: string) => {
    const updatedSlots = documentSlots.map(s => {
      if (s.id === slotId) {
        return { ...s, status: 'PENDING' as const, fileName: undefined, fileUrl: undefined }
      }
      return s
    })

    if (onUpdateDocuments) {
      onUpdateDocuments(updatedSlots)
    }
  }

  const attachedDocs = documentSlots.filter(d => d.fileName || d.status === 'VERIFIED')

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 pb-2 border-b border-gray-200">
        <div>
          <h1 className="font-serif text-3xl font-bold text-[#103a27]">Compliance & KYC Documents</h1>
          <p className="mt-1 text-sm text-[#2a5040]/70">
            Securely upload and manage compliance documents stored in encrypted SACCO storage.
          </p>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-12">
        {/* Upload Panel */}
        <div className="lg:col-span-5 space-y-4">
          <div className="bg-white rounded-2xl p-6 border border-gray-150 shadow-sm space-y-4">
            <h3 className="font-serif text-base font-bold text-[#103a27]">Upload New Document</h3>

            {/* Select Slot */}
            <div className="space-y-1.5">
              <label className="text-xs font-bold uppercase text-gray-500">Document Type</label>
              <select
                value={selectedSlot}
                onChange={(e) => setSelectedSlot(e.target.value)}
                className="w-full rounded-xl border border-gray-200 p-2.5 text-xs font-semibold text-[#103a27] bg-white focus:outline-none focus:border-[#103a27]"
              >
                {documentSlots.map((slot) => (
                  <option key={slot.id} value={slot.id}>
                    {slot.label} {slot.fileName ? '(Attached ✓)' : '(Required)'}
                  </option>
                ))}
              </select>
            </div>

            {/* Drag and Drop Box */}
            <div
              onDragEnter={handleDrag}
              onDragLeave={handleDrag}
              onDragOver={handleDrag}
              onDrop={handleDrop}
              className={`border-2 border-dashed rounded-2xl p-6 text-center transition-all ${
                dragActive ? 'border-[#103a27] bg-[#103a27]/5' : 'border-gray-200 bg-gray-50/50 hover:bg-gray-50'
              }`}
            >
              <Upload className="size-8 mx-auto text-[#103a27] mb-2" />
              <p className="text-xs font-bold text-[#103a27]">
                {isUploading ? 'Uploading file...' : 'Drag & drop your file here'}
              </p>
              <p className="text-[0.65rem] text-gray-400 mt-1">PDF, PNG, JPG (up to 10MB)</p>
              
              <label className="mt-4 inline-flex items-center gap-1 rounded-full bg-[#103a27] text-white px-4 py-2 text-xs font-bold hover:bg-[#1a5235] transition-all cursor-pointer">
                Browse Files
                <input
                  type="file"
                  onChange={handleFileChange}
                  accept=".pdf,.png,.jpg,.jpeg"
                  className="hidden"
                />
              </label>
            </div>
          </div>

          {/* Pending Requirements Card */}
          <div className="bg-white rounded-2xl p-6 border border-gray-150 shadow-sm space-y-3">
            <h4 className="text-xs font-bold uppercase tracking-wider text-gray-500">
              Required Compliance Slots ({pendingSlots.length} Pending)
            </h4>
            <div className="space-y-2">
              {documentSlots.map((slot) => {
                const isAttached = Boolean(slot.fileName || slot.status === 'VERIFIED')
                return (
                  <div key={slot.id} className="flex items-center justify-between p-2.5 rounded-xl bg-gray-50 text-xs">
                    <span className="font-semibold text-gray-700">{slot.label}</span>
                    <span className={`px-2 py-0.5 rounded-full text-[0.65rem] font-bold ${
                      isAttached ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'
                    }`}>
                      {isAttached ? 'Attached ✓' : 'Pending'}
                    </span>
                  </div>
                )
              })}
            </div>
          </div>
        </div>

        {/* Uploaded Documents List */}
        <div className="lg:col-span-7 space-y-4">
          <div className="bg-white rounded-2xl p-6 border border-gray-150 shadow-sm space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-gray-100">
              <div>
                <h3 className="font-serif text-base font-bold text-[#103a27]">Active Attached Documents</h3>
                <p className="text-xs text-gray-500 mt-0.5">
                  Verified documents attached to Application <strong className="font-mono text-gray-700">{application.reference}</strong>
                </p>
              </div>
              <span className="rounded-full bg-emerald-50 text-emerald-800 border border-emerald-200 px-3 py-1 text-xs font-bold flex items-center gap-1">
                <ShieldCheck className="size-3.5" />
                {attachedDocs.length} Verified
              </span>
            </div>

            {attachedDocs.length === 0 ? (
              <div className="py-12 text-center text-gray-400 space-y-2">
                <FileText className="size-8 mx-auto text-gray-300" />
                <p className="text-xs font-semibold text-gray-600">No documents attached yet.</p>
                <p className="text-[0.65rem] text-gray-400">Use the upload box on the left to attach your KYC & proof files.</p>
              </div>
            ) : (
              <div className="space-y-3">
                {attachedDocs.map((doc) => (
                  <div key={doc.id} className="flex items-center justify-between p-4 rounded-xl border border-gray-100 bg-[#f4f5f4] hover:bg-gray-100/60 transition-colors">
                    <div className="flex items-start gap-3">
                      <div className="p-2 rounded-lg bg-white text-[#103a27] shrink-0">
                        <FileText className="size-4 text-[#103a27]" />
                      </div>
                      <div>
                        <p className="text-xs font-bold text-[#103a27]">{doc.label}</p>
                        <p className="text-[0.65rem] font-mono text-emerald-700 mt-0.5">
                          {doc.fileName || `${doc.id}_document.pdf`}
                        </p>
                      </div>
                    </div>

                    <div className="flex items-center gap-2">
                      <button
                        type="button"
                        onClick={() => handleDelete(doc.id)}
                        className="p-1.5 text-gray-400 hover:text-rose-600 rounded-lg hover:bg-white transition-colors"
                        title="Remove file"
                      >
                        <Trash2 className="size-4" />
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
