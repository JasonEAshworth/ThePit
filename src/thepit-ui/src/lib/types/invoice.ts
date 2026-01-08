export interface Invoice {
	id: number;
	invoiceNumber: string;
	amount: number;
	dueDate: string;
	status: InvoiceStatus;
	createdAt: string;
}

export type InvoiceStatus = 'Pending' | 'Paid' | 'Overdue' | 'Cancelled';

export interface CreateInvoiceDto {
	invoiceNumber: string;
	amount: number;
	dueDate: string;
}

export interface UpdateInvoiceDto {
	invoiceNumber?: string;
	amount?: number;
	dueDate?: string;
	status?: InvoiceStatus;
}
