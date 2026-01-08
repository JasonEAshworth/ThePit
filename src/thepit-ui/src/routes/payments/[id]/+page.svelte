<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { paymentApi, invoiceApi } from '$lib/api';
	import type { Payment, Invoice, PaymentStatus } from '$lib/types';

	let payment = $state<Payment | null>(null);
	let invoice = $state<Invoice | null>(null);
	let loading = $state(true);
	let error = $state<string | null>(null);
	let deleting = $state(false);

	// Update status
	let showStatusModal = $state(false);
	let newStatus = $state<PaymentStatus>('Pending');
	let updatingStatus = $state(false);

	const statuses: PaymentStatus[] = ['Pending', 'Completed', 'Failed', 'Refunded'];
	const paymentId = $derived(Number($page.params.id));

	onMount(async () => {
		await loadData();
	});

	async function loadData() {
		loading = true;
		error = null;
		try {
			payment = await paymentApi.getById(paymentId);
			if (payment) {
				newStatus = payment.status;
				try {
					invoice = await invoiceApi.getById(payment.invoiceId);
				} catch {
					// Invoice may not exist
				}
			}
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to load payment';
		} finally {
			loading = false;
		}
	}

	async function handleDelete() {
		if (!confirm('Are you sure you want to delete this payment?')) return;

		deleting = true;
		try {
			await paymentApi.delete(paymentId);
			goto('/payments');
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to delete payment';
			deleting = false;
		}
	}

	async function handleUpdateStatus() {
		updatingStatus = true;
		try {
			await paymentApi.updateStatus(paymentId, { status: newStatus });
			showStatusModal = false;
			await loadData();
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to update status';
		} finally {
			updatingStatus = false;
		}
	}

	function formatDateTime(dateStr: string): string {
		return new Date(dateStr).toLocaleString();
	}

	function formatCurrency(amount: number): string {
		return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);
	}

	function getStatusClass(status: string): string {
		switch (status) {
			case 'Completed':
			case 'Paid':
				return 'bg-green-100 text-green-800';
			case 'Pending':
				return 'bg-yellow-100 text-yellow-800';
			case 'Failed':
			case 'Overdue':
				return 'bg-red-100 text-red-800';
			case 'Refunded':
			case 'Cancelled':
				return 'bg-gray-100 text-gray-800';
			default:
				return 'bg-gray-100 text-gray-800';
		}
	}
</script>

<div class="p-6">
	<div class="mb-6">
		<a href="/payments" class="text-blue-600 hover:underline">&larr; Back to Payments</a>
	</div>

	{#if loading}
		<div class="flex items-center justify-center py-12">
			<div class="text-gray-500">Loading payment...</div>
		</div>
	{:else if error}
		<div class="rounded-md bg-red-50 p-4 text-red-700">
			{error}
			<button onclick={loadData} class="ml-4 underline">Retry</button>
		</div>
	{:else if payment}
		<!-- Payment Header -->
		<div class="mb-6 flex items-start justify-between">
			<div>
				<h1 class="text-2xl font-bold text-gray-900">Payment {payment.transactionId}</h1>
				<span
					class="mt-2 inline-flex rounded-full px-3 py-1 text-sm font-semibold {getStatusClass(payment.status)}"
				>
					{payment.status}
				</span>
			</div>
			<div class="flex gap-2">
				<button
					onclick={() => (showStatusModal = true)}
					class="rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
				>
					Update Status
				</button>
				<button
					onclick={handleDelete}
					disabled={deleting}
					class="rounded-md bg-red-600 px-4 py-2 text-white hover:bg-red-700 disabled:opacity-50"
				>
					{deleting ? 'Deleting...' : 'Delete'}
				</button>
			</div>
		</div>

		<!-- Payment Details -->
		<div class="grid gap-6 md:grid-cols-2">
			<div class="rounded-lg border border-gray-200 bg-white p-6">
				<h2 class="mb-4 text-lg font-semibold text-gray-900">Payment Details</h2>
				<dl class="space-y-3">
					<div class="flex justify-between">
						<dt class="text-gray-500">Transaction ID</dt>
						<dd class="font-mono text-gray-900">{payment.transactionId}</dd>
					</div>
					<div class="flex justify-between">
						<dt class="text-gray-500">Amount</dt>
						<dd class="font-medium text-gray-900">{formatCurrency(payment.amount)}</dd>
					</div>
					<div class="flex justify-between">
						<dt class="text-gray-500">Payment Date</dt>
						<dd class="text-gray-900">{formatDateTime(payment.paymentDate)}</dd>
					</div>
					<div class="flex justify-between">
						<dt class="text-gray-500">Payment Method</dt>
						<dd class="text-gray-900">{payment.paymentMethod}</dd>
					</div>
					<div class="flex justify-between">
						<dt class="text-gray-500">Status</dt>
						<dd>
							<span
								class="inline-flex rounded-full px-2 text-xs font-semibold {getStatusClass(payment.status)}"
							>
								{payment.status}
							</span>
						</dd>
					</div>
				</dl>
			</div>

			<div class="rounded-lg border border-gray-200 bg-white p-6">
				<h2 class="mb-4 text-lg font-semibold text-gray-900">Linked Invoice</h2>
				{#if invoice}
					<dl class="space-y-3">
						<div class="flex justify-between">
							<dt class="text-gray-500">Invoice Number</dt>
							<dd>
								<a href="/invoices/{invoice.id}" class="text-blue-600 hover:underline">
									{invoice.invoiceNumber}
								</a>
							</dd>
						</div>
						<div class="flex justify-between">
							<dt class="text-gray-500">Invoice Amount</dt>
							<dd class="text-gray-900">{formatCurrency(invoice.amount)}</dd>
						</div>
						<div class="flex justify-between">
							<dt class="text-gray-500">Invoice Status</dt>
							<dd>
								<span
									class="inline-flex rounded-full px-2 text-xs font-semibold {getStatusClass(invoice.status)}"
								>
									{invoice.status}
								</span>
							</dd>
						</div>
					</dl>
				{:else}
					<p class="text-gray-500">Invoice #{payment.invoiceId}</p>
					<a href="/invoices/{payment.invoiceId}" class="text-blue-600 hover:underline">
						View Invoice
					</a>
				{/if}
			</div>
		</div>
	{/if}
</div>

<!-- Status Update Modal -->
{#if showStatusModal}
	<div class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
		<div class="w-full max-w-md rounded-lg bg-white p-6 shadow-xl">
			<h2 class="mb-4 text-xl font-semibold">Update Payment Status</h2>

			<div class="mb-6">
				<label for="status" class="mb-1 block text-sm font-medium text-gray-700">Status</label>
				<select
					id="status"
					bind:value={newStatus}
					class="w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:outline-none"
				>
					{#each statuses as status}
						<option value={status}>{status}</option>
					{/each}
				</select>
			</div>

			<div class="flex justify-end gap-2">
				<button
					onclick={() => (showStatusModal = false)}
					class="rounded-md bg-gray-100 px-4 py-2 text-gray-700 hover:bg-gray-200"
				>
					Cancel
				</button>
				<button
					onclick={handleUpdateStatus}
					disabled={updatingStatus}
					class="rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:opacity-50"
				>
					{updatingStatus ? 'Updating...' : 'Update Status'}
				</button>
			</div>
		</div>
	</div>
{/if}
