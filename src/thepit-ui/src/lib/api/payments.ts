import { apiClient } from './client';
import type {
	Payment,
	CreatePaymentDto,
	ProcessPaymentRequest,
	UpdatePaymentStatusRequest
} from '$lib/types';

export const paymentApi = {
	getAll(): Promise<Payment[]> {
		return apiClient.get<Payment[]>('/payment');
	},

	getById(id: number): Promise<Payment> {
		return apiClient.get<Payment>(`/payment/${id}`);
	},

	getByTransactionId(transactionId: string): Promise<Payment> {
		return apiClient.get<Payment>(`/payment/transaction/${transactionId}`);
	},

	getByInvoiceId(invoiceId: number): Promise<Payment[]> {
		return apiClient.get<Payment[]>(`/payment/invoice/${invoiceId}`);
	},

	create(dto: CreatePaymentDto): Promise<Payment> {
		return apiClient.post<Payment, CreatePaymentDto>('/payment', dto);
	},

	processPayment(request: ProcessPaymentRequest): Promise<Payment> {
		return apiClient.post<Payment, ProcessPaymentRequest>('/payment/process', request);
	},

	updateStatus(id: number, request: UpdatePaymentStatusRequest): Promise<Payment> {
		return apiClient.put<Payment, UpdatePaymentStatusRequest>(`/payment/${id}/status`, request);
	},

	delete(id: number): Promise<void> {
		return apiClient.delete(`/payment/${id}`);
	}
};
