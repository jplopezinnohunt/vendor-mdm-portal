import React, { useState, useRef } from 'react';
import { Upload, File, X, Download, Loader2, AlertCircle } from 'lucide-react';
import { api } from '../../services/api';

interface AttachmentMetadata {
    fileName: string;
    blobName: string;
    contentType: string;
    sizeBytes: number;
    uploadedAt: string;
    category: string;
    scanStatus?: 'pending_scan' | 'clean' | 'infected';
}

interface FileUploadProps {
    label: string;
    category: string;
    maxFiles?: number;
    accept?: string;
    vendorId: string;
    onUploadComplete: (metadata: AttachmentMetadata) => void;
    onDelete: (blobName: string) => void;
    existingFiles?: AttachmentMetadata[];
}

/**
 * File Upload Component - Gold Standard Direct Upload Pattern
 * Implements SAS token upload directly to Azure Blob Storage
 */
export const FileUpload: React.FC<FileUploadProps> = ({
    label,
    category,
    maxFiles = 5,
    accept = 'application/pdf,image/jpeg,image/png,application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    vendorId,
    onUploadComplete,
    onDelete,
    existingFiles = []
}) => {
    const [uploading, setUploading] = useState(false);
    const [uploadProgress, setUploadProgress] = useState(0);
    const [error, setError] = useState<string | null>(null);
    const [dragActive, setDragActive] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const formatFileSize = (bytes: number): string => {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    };

    const validateFile = (file: File): string | null => {
        // Validate file type
        const allowedTypes = accept.split(',');
        if (!allowedTypes.includes(file.type)) {
            return `File type not allowed. Accepted types: ${allowedTypes.join(', ')}`;
        }

        // Validate file size (10MB max)
        const maxSize = 10 * 1024 * 1024;
        if (file.size > maxSize) {
            return `File size exceeds 10MB limit (${formatFileSize(file.size)})`;
        }

        // Check max files
        if (existingFiles.length >= maxFiles) {
            return `Maximum ${maxFiles} files allowed`;
        }

        return null;
    };

    const handleUpload = async (file: File) => {
        setError(null);
        setUploading(true);
        setUploadProgress(0);

        try {
            const validationError = validateFile(file);
            if (validationError) {
                setError(validationError);
                setUploading(false);
                return;
            }

            // Step 1: Request upload permission (SAS token)
            setUploadProgress(10);
            const uploadRequest = await api.post('/attachments/request-upload', {
                fileName: file.name,
                contentType: file.type,
                category,
                sizeBytes: file.size,
                vendorId
            });

            const { sasUrl, blobName } = uploadRequest.data;

            // Step 2: Upload directly to Azure Blob Storage
            setUploadProgress(30);
            const uploadResponse = await fetch(sasUrl, {
                method: 'PUT',
                headers: {
                    'x-ms-blob-type': 'BlockBlob',
                    'Content-Type': file.type
                },
                body: file
            });

            if (!uploadResponse.ok) {
                throw new Error('Failed to upload file to storage');
            }

            setUploadProgress(80);

            // Step 3: Confirm upload to backend
            const confirmResponse = await api.post('/attachments/confirm-upload', {
                blobName,
                vendorId
            });

            const { metadata } = confirmResponse.data;

            setUploadProgress(100);
            onUploadComplete(metadata);

            // Reset state
            setTimeout(() => {
                setUploading(false);
                setUploadProgress(0);
            }, 500);
        } catch (err: any) {
            console.error('Upload error:', err);
            setError(err.response?.data?.error || err.message || 'Upload failed');
            setUploading(false);
            setUploadProgress(0);
        }
    };

    const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (file) {
            handleUpload(file);
        }
        // Reset input so same file can be selected again
        event.target.value = '';
    };

    const handleDrop = (event: React.DragEvent<HTMLDivElement>) => {
        event.preventDefault();
        event.stopPropagation();
        setDragActive(false);

        const file = event.dataTransfer.files?.[0];
        if (file) {
            handleUpload(file);
        }
    };

    const handleDrag = (event: React.DragEvent<HTMLDivElement>) => {
        event.preventDefault();
        event.stopPropagation();
        if (event.type === 'dragenter' || event.type === 'dragover') {
            setDragActive(true);
        } else if (event.type === 'dragleave') {
            setDragActive(false);
        }
    };

    const handleDownload = async (attachment: AttachmentMetadata) => {
        try {
            const response = await api.get(`/attachments/${encodeURIComponent(attachment.blobName)}/download-url`);
            const { downloadUrl } = response.data;

            // Open download URL in new tab
            window.open(downloadUrl, '_blank');
        } catch (err: any) {
            console.error('Download error:', err);
            setError('Failed to generate download link');
        }
    };

    const handleDelete = async (blobName: string) => {
        if (!confirm('Are you sure you want to delete this file?')) {
            return;
        }

        try {
            await api.delete(`/attachments/${encodeURIComponent(blobName)}`);
            onDelete(blobName);
        } catch (err: any) {
            console.error('Delete error:', err);
            setError('Failed to delete file');
        }
    };

    return (
        <div className="space-y-4">
            {/* Label */}
            <label className="block text-xs font-medium text-blue-700">
                {label} {maxFiles > 1 && `(Max ${maxFiles})`}
            </label>

            {/* Upload Area */}
            <div
                className={`border-2 border-dashed rounded-lg p-6 text-center transition-colors ${dragActive
                        ? 'border-blue-500 bg-blue-50'
                        : uploading
                            ? 'border-gray-300 bg-gray-50'
                            : 'border-gray-300 hover:border-blue-400 hover:bg-blue-50 cursor-pointer'
                    }`}
                onDragEnter={handleDrag}
                onDragOver={handleDrag}
                onDragLeave={handleDrag}
                onDrop={handleDrop}
                onClick={() => !uploading && fileInputRef.current?.click()}
            >
                <input
                    ref={fileInputRef}
                    type="file"
                    accept={accept}
                    onChange={handleFileSelect}
                    className="hidden"
                    disabled={uploading || existingFiles.length >= maxFiles}
                />

                {uploading ? (
                    <div className="space-y-3">
                        <Loader2 className="w-8 h-8 mx-auto text-blue-600 animate-spin" />
                        <p className="text-sm text-gray-600">Uploading... {uploadProgress}%</p>
                        <div className="w-full bg-gray-200 rounded-full h-2">
                            <div
                                className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                                style={{ width: `${uploadProgress}%` }}
                            />
                        </div>
                    </div>
                ) : (
                    <div className="space-y-2">
                        <Upload className="w-8 h-8 mx-auto text-gray-400" />
                        <p className="text-sm text-gray-600">
                            <span className="text-blue-600 font-medium">Click to upload</span> or drag and drop
                        </p>
                        <p className="text-xs text-gray-500">
                            PDF, JPG, PNG, DOCX (Max 10MB)
                        </p>
                    </div>
                )}
            </div>

            {/* Error Message */}
            {error && (
                <div className="flex items-center gap-2 p-3 bg-red-50 border border-red-200 rounded text-sm text-red-700">
                    <AlertCircle className="w-4 h-4 flex-shrink-0" />
                    <span>{error}</span>
                </div>
            )}

            {/* Existing Files List */}
            {existingFiles.length > 0 && (
                <div className="space-y-2">
                    <p className="text-xs font-medium text-gray-500 uppercase">Uploaded Files</p>
                    {existingFiles.map((file) => (
                        <div
                            key={file.blobName}
                            className="flex items-center justify-between p-3 bg-gray-50 border border-gray-200 rounded hover:bg-gray-100 transition-colors"
                        >
                            <div className="flex items-center gap-3 flex-1 min-w-0">
                                <File className="w-4 h-4 text-gray-500 flex-shrink-0" />
                                <div className="flex-1 min-w-0">
                                    <p className="text-sm font-medium text-gray-900 truncate">{file.fileName}</p>
                                    <p className="text-xs text-gray-500">
                                        {formatFileSize(file.sizeBytes)}
                                        {file.scanStatus && (
                                            <span
                                                className={`ml-2 ${file.scanStatus === 'clean'
                                                        ? 'text-green-600'
                                                        : file.scanStatus === 'infected'
                                                            ? 'text-red-600'
                                                            : 'text-yellow-600'
                                                    }`}
                                            >
                                                • {file.scanStatus === 'clean' ? 'Verified' : file.scanStatus === 'infected' ? 'Quarantined' : 'Scanning...'}
                                            </span>
                                        )}
                                    </p>
                                </div>
                            </div>
                            <div className="flex items-center gap-2">
                                <button
                                    type="button"
                                    onClick={() => handleDownload(file)}
                                    className="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors"
                                    title="Download"
                                >
                                    <Download className="w-4 h-4" />
                                </button>
                                <button
                                    type="button"
                                    onClick={() => handleDelete(file.blobName)}
                                    className="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors"
                                    title="Delete"
                                >
                                    <X className="w-4 h-4" />
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};
