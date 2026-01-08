export interface Payment {
	id: number;
	invoiceId: number;
	amount: number;
	paymentDate: string;
	paymentMethod: string;
	transactionId: string;
	status: PaymentStatus;
}

export type PaymentStatus = 'Pending' | 'Completed' | 'Failed' | 'Refunded';

export interface CreatePaymentDto {
	invoiceId: number;
	amount: number;
	paymentMethod: string;
}

export interface ProcessPaymentRequest {
	invoiceId: number;
	amount: number;
	paymentMethod: string;
}

export interface UpdatePaymentStatusRequest {
	status: PaymentStatus;
}
