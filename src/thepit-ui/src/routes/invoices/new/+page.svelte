<script lang="ts">
	import { goto } from '$app/navigation';
	import { invoiceApi } from '$lib/api';
	import { InvoiceForm, ErrorAlert } from '$lib/components';
	import type { CreateInvoiceDto } from '$lib/types';

	let error = $state<string | null>(null);

	async function handleSubmit(data: CreateInvoiceDto) {
		error = null;
		try {
			await invoiceApi.create(data);
			goto('/invoices');
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to create invoice';
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
		<h1 class="mt-2 text-2xl font-bold text-gray-900">Create Invoice</h1>
	</div>

	{#if error}
		<ErrorAlert message={error} onDismiss={() => (error = null)} />
	{/if}

	<div class="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
		<InvoiceForm onSubmit={handleSubmit} onCancel={handleCancel} />
	</div>
</div>
