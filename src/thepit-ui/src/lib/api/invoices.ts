import { apiClient } from './client';
import type { Invoice, CreateInvoiceDto, UpdateInvoiceDto } from '$lib/types';

export const invoiceApi = {
	getAll(): Promise<Invoice[]> {
		return apiClient.get<Invoice[]>('/invoice');
	},

	getById(id: number): Promise<Invoice> {
		return apiClient.get<Invoice>(`/invoice/${id}`);
	},

	create(dto: CreateInvoiceDto): Promise<Invoice> {
		return apiClient.post<Invoice, CreateInvoiceDto>('/invoice', dto);
	},

	update(id: number, dto: UpdateInvoiceDto): Promise<Invoice> {
		return apiClient.put<Invoice, UpdateInvoiceDto>(`/invoice/${id}`, dto);
	},

	delete(id: number): Promise<void> {
		return apiClient.delete(`/invoice/${id}`);
	}
};
