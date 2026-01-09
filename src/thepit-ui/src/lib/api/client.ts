import { env } from '$env/dynamic/public';
import type { ApiError } from '$lib/types';

export class ApiClient {
	private baseUrl: string;

	constructor(baseUrl: string = env.PUBLIC_API_BASE_URL ?? '') {
		this.baseUrl = baseUrl.replace(/\/$/, '');
	}

	async get<T>(path: string): Promise<T> {
		const response = await fetch(`${this.baseUrl}${path}`, {
			method: 'GET',
			headers: {
				'Content-Type': 'application/json'
			}
		});
		return this.handleResponse<T>(response);
	}

	async post<T, D>(path: string, data: D): Promise<T> {
		const response = await fetch(`${this.baseUrl}${path}`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify(data)
		});
		return this.handleResponse<T>(response);
	}

	async put<T, D>(path: string, data: D): Promise<T> {
		const response = await fetch(`${this.baseUrl}${path}`, {
			method: 'PUT',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify(data)
		});
		return this.handleResponse<T>(response);
	}

	async delete(path: string): Promise<void> {
		const response = await fetch(`${this.baseUrl}${path}`, {
			method: 'DELETE'
		});
		if (!response.ok) {
			await this.handleError(response);
		}
	}

	private async handleResponse<T>(response: Response): Promise<T> {
		if (!response.ok) {
			await this.handleError(response);
		}
		return response.json() as Promise<T>;
	}

	private async handleError(response: Response): Promise<never> {
		let message = `HTTP ${response.status}: ${response.statusText}`;
		try {
			const error = (await response.json()) as ApiError;
			if (error.error) {
				message = error.error;
			}
		} catch {
			// Use default message if response is not JSON
		}
		throw new Error(message);
	}
}

export const apiClient = new ApiClient();
