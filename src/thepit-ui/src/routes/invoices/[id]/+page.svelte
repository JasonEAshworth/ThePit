<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { invoiceApi, paymentApi } from '$lib/api';
	import type { Invoice, Payment } from '$lib/types';

	let invoice = $state<Invoice | null>(null);
	let payments = $state<Payment[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);
	let deleting = $state(false);

	// Process payment modal
	let showPaymentModal = $state(false);
	let paymentAmount = $state(0);
	let paymentMethod = $state('Credit Card');
	let processingPayment = $state(false);

	const invoiceId = $derived(Number($page.params.id));

	onMount(async () => {
		await loadData();
	});

	async function loadData() {
		loading = true;
		error = null;
		try {
			const [inv, pays] = await Promise.all([
				invoiceApi.getById(invoiceId),
				paymentApi.getByInvoiceId(invoiceId)
			]);
			invoice = inv;
			payments = pays;
			paymentAmount = inv.amount - payments.reduce((sum, p) => sum + p.amount, 0);
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to load invoice';
		} finally {
			loading = false;
		}
	}

	async function handleDelete() {
		if (!confirm('Are you sure you want to delete this invoice?')) return;

		deleting = true;
		try {
			await invoiceApi.delete(invoiceId);
			goto('/invoices');
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to delete invoice';
			deleting = false;
		}
	}

	async function handleProcessPayment() {
		processingPayment = true;
		try {
			await paymentApi.processPayment({
				invoiceId,
				amount: paymentAmount,
				paymentMethod
			});
			showPaymentModal = false;
			await loadData();
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to process payment';
		} finally {
			processingPayment = false;
		}
	}

	function formatDate(dateStr: string): string {
		return new Date(dateStr).toLocaleDateString();
	}

	function formatDateTime(dateStr: string): string {
		return new Date(dateStr).toLocaleString();
	}

	function formatCurrency(amount: number): string {
		return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);
	}

	function getStatusClass(status: string): string {
		switch (status) {
			case 'Paid':
			case 'Completed':
				return 'bg-green-100 text-green-800';
			case 'Pending':
				return 'bg-yellow-100 text-yellow-800';
			case 'Overdue':
			case 'Failed':
				return 'bg-red-100 text-red-800';
			case 'Cancelled':
			case 'Refunded':
				return 'bg-gray-100 text-gray-800';
			default:
				return 'bg-gray-100 text-gray-800';
		}
	}

	const totalPaid = $derived(payments.reduce((sum, p) => sum + p.amount, 0));
	const remainingBalance = $derived(invoice ? invoice.amount - totalPaid : 0);
</script>

<div class="p-6">
	<div class="mb-6">
		<a href="/invoices" class="text-blue-600 hover:underline">&larr; Back to Invoices</a>
	</div>

	{#if loading}
		<div class="flex items-center justify-center py-12">
			<div class="text-gray-500">Loading invoice...</div>
		</div>
	{:else if error}
		<div class="rounded-md bg-red-50 p-4 text-red-700">
			{error}
			<button onclick={loadData} class="ml-4 underline">Retry</button>
		</div>
	{:else if invoice}
		<!-- Invoice Header -->
		<div class="mb-6 flex items-start justify-between">
			<div>
				<h1 class="text-2xl font-bold text-gray-900">Invoice {invoice.invoiceNumber}</h1>
				<span
					class="mt-2 inline-flex rounded-full px-3 py-1 text-sm font-semibold {getStatusClass(invoice.status)}"
				>
					{invoice.status}
				</span>
			</div>
			<div class="flex gap-2">
				<a
					href="/invoices/{invoice.id}/edit"
					class="rounded-md bg-gray-100 px-4 py-2 text-gray-700 hover:bg-gray-200"
				>
					Edit
				</a>
				{#if invoice.status !== 'Paid' && invoice.status !== 'Cancelled'}
					<button
						onclick={() => (showPaymentModal = true)}
						class="rounded-md bg-green-600 px-4 py-2 text-white hover:bg-green-700"
					>
						Process Payment
					</button>
				{/if}
				<button
					onclick={handleDelete}
					disabled={deleting}
					class="rounded-md bg-red-600 px-4 py-2 text-white hover:bg-red-700 disabled:opacity-50"
				>
					{deleting ? 'Deleting...' : 'Delete'}
				</button>
			</div>
		</div>

		<!-- Invoice Details -->
		<div class="mb-8 grid gap-6 md:grid-cols-2">
			<div class="rounded-lg border border-gray-200 bg-white p-6">
				<h2 class="mb-4 text-lg font-semibold text-gray-900">Invoice Details</h2>
				<dl class="space-y-3">
					<div class="flex justify-between">
						<dt class="text-gray-500">Amount</dt>
						<dd class="font-medium text-gray-900">{formatCurrency(invoice.amount)}</dd>
					</div>
					<div class="flex justify-between">
						<dt class="text-gray-500">Due Date</dt>
						<dd class="text-gray-900">{formatDate(invoice.dueDate)}</dd>
					</div>
					<div class="flex justify-between">
						<dt class="text-gray-500">Created</dt>
						<dd class="text-gray-900">{formatDateTime(invoice.createdAt)}</dd>
					</div>
				</dl>
			</div>

			<div class="rounded-lg border border-gray-200 bg-white p-6">
				<h2 class="mb-4 text-lg font-semibold text-gray-900">Payment Summary</h2>
				<dl class="space-y-3">
					<div class="flex justify-between">
						<dt class="text-gray-500">Total Amount</dt>
						<dd class="text-gray-900">{formatCurrency(invoice.amount)}</dd>
					</div>
					<div class="flex justify-between">
						<dt class="text-gray-500">Paid</dt>
						<dd class="text-green-600">{formatCurrency(totalPaid)}</dd>
					</div>
					<div class="flex justify-between border-t pt-3">
						<dt class="font-medium text-gray-900">Remaining</dt>
						<dd class="font-medium {remainingBalance > 0 ? 'text-red-600' : 'text-green-600'}">
							{formatCurrency(remainingBalance)}
						</dd>
					</div>
				</dl>
			</div>
		</div>

		<!-- Payments -->
		<div class="rounded-lg border border-gray-200 bg-white p-6">
			<h2 class="mb-4 text-lg font-semibold text-gray-900">Payments</h2>
			{#if payments.length === 0}
				<p class="text-gray-500">No payments recorded for this invoice.</p>
			{:else}
				<div class="overflow-x-auto">
					<table class="min-w-full divide-y divide-gray-200">
						<thead>
							<tr>
								<th class="px-4 py-2 text-left text-xs font-medium uppercase text-gray-500">
									Date
								</th>
								<th class="px-4 py-2 text-left text-xs font-medium uppercase text-gray-500">
									Amount
								</th>
								<th class="px-4 py-2 text-left text-xs font-medium uppercase text-gray-500">
									Method
								</th>
								<th class="px-4 py-2 text-left text-xs font-medium uppercase text-gray-500">
									Transaction ID
								</th>
								<th class="px-4 py-2 text-left text-xs font-medium uppercase text-gray-500">
									Status
								</th>
							</tr>
						</thead>
						<tbody class="divide-y divide-gray-200">
							{#each payments as payment (payment.id)}
								<tr>
									<td class="whitespace-nowrap px-4 py-2 text-sm text-gray-900">
										{formatDateTime(payment.paymentDate)}
									</td>
									<td class="whitespace-nowrap px-4 py-2 text-sm text-gray-900">
										{formatCurrency(payment.amount)}
									</td>
									<td class="whitespace-nowrap px-4 py-2 text-sm text-gray-500">
										{payment.paymentMethod}
									</td>
									<td class="whitespace-nowrap px-4 py-2 text-sm text-gray-500">
										{payment.transactionId}
									</td>
									<td class="whitespace-nowrap px-4 py-2 text-sm">
										<span
											class="inline-flex rounded-full px-2 text-xs font-semibold {getStatusClass(payment.status)}"
										>
											{payment.status}
										</span>
									</td>
								</tr>
							{/each}
						</tbody>
					</table>
				</div>
			{/if}
		</div>
	{/if}
</div>

<!-- Payment Modal -->
{#if showPaymentModal}
	<div class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
		<div class="w-full max-w-md rounded-lg bg-white p-6 shadow-xl">
			<h2 class="mb-4 text-xl font-semibold">Process Payment</h2>

			<div class="mb-4">
				<label for="amount" class="mb-1 block text-sm font-medium text-gray-700">Amount</label>
				<input
					id="amount"
					type="number"
					step="0.01"
					min="0.01"
					max={remainingBalance}
					bind:value={paymentAmount}
					class="w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
				/>
			</div>

			<div class="mb-6">
				<label for="method" class="mb-1 block text-sm font-medium text-gray-700">
					Payment Method
				</label>
				<select
					id="method"
					bind:value={paymentMethod}
					class="w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
				>
					<option>Credit Card</option>
					<option>Debit Card</option>
					<option>Bank Transfer</option>
					<option>Cash</option>
					<option>Check</option>
				</select>
			</div>

			<div class="flex justify-end gap-2">
				<button
					onclick={() => (showPaymentModal = false)}
					class="rounded-md bg-gray-100 px-4 py-2 text-gray-700 hover:bg-gray-200"
				>
					Cancel
				</button>
				<button
					onclick={handleProcessPayment}
					disabled={processingPayment || paymentAmount <= 0}
					class="rounded-md bg-green-600 px-4 py-2 text-white hover:bg-green-700 disabled:opacity-50"
				>
					{processingPayment ? 'Processing...' : 'Process Payment'}
				</button>
			</div>
		</div>
	</div>
{/if}
