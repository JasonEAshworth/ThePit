<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { onMount } from 'svelte';
	import { invoiceApi } from '$lib/api';
	import { InvoiceForm, LoadingSpinner, ErrorAlert } from '$lib/components';
	import type { Invoice, UpdateInvoiceDto } from '$lib/types';

	let invoice = $state<Invoice | null>(null);
	let loading = $state(true);
	let error = $state<string | null>(null);
	let submitError = $state<string | null>(null);

	const invoiceId = $derived(parseInt($page.params.id, 10));

	onMount(async () => {
		await loadInvoice();
	});

	async function loadInvoice() {
		loading = true;
		error = null;
		try {
			invoice = await invoiceApi.getById(invoiceId);
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to load invoice';
		} finally {
			loading = false;
		}
	}

	async function handleSubmit(data: UpdateInvoiceDto) {
		submitError = null;
		try {
			await invoiceApi.update(invoiceId, data);
			goto('/invoices');
		} catch (e) {
			submitError = e instanceof Error ? e.message : 'Failed to update invoice';
			throw e;
		}
	}

	function handleCancel() {
		goto('/invoices');
	}
</script>

<div class="mx-auto max-w-2xl p-6">
	<div class="mb-6">
		<a href="/invoices" class="text-sm text-blue-600 hover:underline">&larr; Back to Invoices</a>
		<h1 class="mt-2 text-2xl font-bold text-gray-900">Edit Invoice</h1>
	</div>

	{#if loading}
		<LoadingSpinner message="Loading invoice..." />
	{:else if error}
		<ErrorAlert message={error} onRetry={loadInvoice} />
	{:else if invoice}
		{#if submitError}
			<ErrorAlert message={submitError} onDismiss={() => (submitError = null)} />
		{/if}

		<div class="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
			<InvoiceForm {invoice} onSubmit={handleSubmit} onCancel={handleCancel} isEdit={true} />
		</div>
	{/if}
</div>
