<script lang="ts">
	import type { Invoice, InvoiceStatus, CreateInvoiceDto, UpdateInvoiceDto } from '$lib/types';

	interface Props {
		invoice?: Invoice;
		onSubmit: (data: CreateInvoiceDto | UpdateInvoiceDto) => Promise<void>;
		onCancel: () => void;
		isEdit?: boolean;
	}

	let { invoice, onSubmit, onCancel, isEdit = false }: Props = $props();

	const statuses: InvoiceStatus[] = ['Pending', 'Paid', 'Overdue', 'Cancelled'];

	let invoiceNumber = $state(invoice?.invoiceNumber ?? '');
	let amount = $state(invoice?.amount?.toString() ?? '');
	let dueDate = $state(invoice?.dueDate ? invoice.dueDate.split('T')[0] : '');
	let status = $state<InvoiceStatus>(invoice?.status ?? 'Pending');

	let submitting = $state(false);
	let errors = $state<Record<string, string>>({});

	function validate(): boolean {
		const newErrors: Record<string, string> = {};

		if (!invoiceNumber.trim()) {
			newErrors.invoiceNumber = 'Invoice number is required';
		}

		const amountNum = parseFloat(amount);
		if (!amount || isNaN(amountNum)) {
			newErrors.amount = 'Valid amount is required';
		} else if (amountNum <= 0) {
			newErrors.amount = 'Amount must be greater than zero';
		}

		if (!dueDate) {
			newErrors.dueDate = 'Due date is required';
		}

		errors = newErrors;
		return Object.keys(newErrors).length === 0;
	}

	async function handleSubmit(e: Event) {
		e.preventDefault();

		if (!validate()) return;

		submitting = true;
		try {
			const data: CreateInvoiceDto | UpdateInvoiceDto = {
				invoiceNumber: invoiceNumber.trim(),
				amount: parseFloat(amount),
				dueDate: dueDate
			};

			if (isEdit) {
				(data as UpdateInvoiceDto).status = status;
			}

			await onSubmit(data);
		} finally {
			submitting = false;
		}
	}
</script>

<form onsubmit={handleSubmit} class="space-y-6">
	<div>
		<label for="invoiceNumber" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
			Invoice Number
		</label>
		<input
			type="text"
			id="invoiceNumber"
			bind:value={invoiceNumber}
			class="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-pit-500 focus:outline-none focus:ring-1 focus:ring-pit-500 dark:border-slate-600 dark:bg-slate-700 dark:text-white dark:placeholder-gray-400"
			class:border-red-500={errors.invoiceNumber}
			class:dark:border-red-500={errors.invoiceNumber}
			placeholder="INV-001"
		/>
		{#if errors.invoiceNumber}
			<p class="mt-1 text-sm text-red-600 dark:text-red-400">{errors.invoiceNumber}</p>
		{/if}
	</div>

	<div>
		<label for="amount" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Amount</label>
		<div class="relative mt-1">
			<span class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3 text-gray-500 dark:text-gray-400">
				$
			</span>
			<input
				type="number"
				id="amount"
				bind:value={amount}
				step="0.01"
				min="0"
				class="block w-full rounded-md border border-gray-300 py-2 pl-7 pr-3 shadow-sm focus:border-pit-500 focus:outline-none focus:ring-1 focus:ring-pit-500 dark:border-slate-600 dark:bg-slate-700 dark:text-white dark:placeholder-gray-400"
				class:border-red-500={errors.amount}
				class:dark:border-red-500={errors.amount}
				placeholder="0.00"
			/>
		</div>
		{#if errors.amount}
			<p class="mt-1 text-sm text-red-600 dark:text-red-400">{errors.amount}</p>
		{/if}
	</div>

	<div>
		<label for="dueDate" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Due Date</label>
		<input
			type="date"
			id="dueDate"
			bind:value={dueDate}
			class="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-pit-500 focus:outline-none focus:ring-1 focus:ring-pit-500 dark:border-slate-600 dark:bg-slate-700 dark:text-white"
			class:border-red-500={errors.dueDate}
			class:dark:border-red-500={errors.dueDate}
		/>
		{#if errors.dueDate}
			<p class="mt-1 text-sm text-red-600 dark:text-red-400">{errors.dueDate}</p>
		{/if}
	</div>

	{#if isEdit}
		<div>
			<label for="status" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Status</label>
			<select
				id="status"
				bind:value={status}
				class="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-pit-500 focus:outline-none focus:ring-1 focus:ring-pit-500 dark:border-slate-600 dark:bg-slate-700 dark:text-white"
			>
				{#each statuses as s}
					<option value={s}>{s}</option>
				{/each}
			</select>
		</div>
	{/if}

	<div class="flex justify-end gap-3 pt-4">
		<button
			type="button"
			onclick={onCancel}
			class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-pit-500 focus:ring-offset-2 dark:border-slate-600 dark:bg-slate-700 dark:text-gray-200 dark:hover:bg-slate-600 dark:focus:ring-offset-slate-900"
		>
			Cancel
		</button>
		<button
			type="submit"
			disabled={submitting}
			class="rounded-md bg-pit-500 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-pit-600 focus:outline-none focus:ring-2 focus:ring-pit-500 focus:ring-offset-2 disabled:opacity-50 dark:focus:ring-offset-slate-900"
		>
			{submitting ? 'Saving...' : isEdit ? 'Update Invoice' : 'Create Invoice'}
		</button>
	</div>
</form>
