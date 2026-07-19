import type { ApiEnvelope } from "@/lib/apiClient";
import { getApiOrigin } from "@/lib/apiClient";

function joinUrl(path: string): string {
  const p = path.startsWith("/") ? path : `/${path}`;
  return `${getApiOrigin()}/api/v1${p}`;
}

function buildEnvelopeError(
  env: { message?: string | null; errors?: string[] | null },
  status?: number,
): Error {
  const errors = (env.errors ?? []).filter(Boolean);
  const code = errors[0];
  const message = env.message?.trim() || code || "Request failed";
  const err = new Error(message) as Error & { code?: string; errors?: string[]; status?: number };
  if (code) err.code = code;
  if (errors.length) err.errors = errors;
  if (status !== undefined) err.status = status;
  return err;
}

async function parseResponse<T>(res: Response): Promise<T | undefined> {
  if (res.status === 204) return undefined;
  const text = await res.text();
  if (!text) return undefined;
  const json = JSON.parse(text) as ApiEnvelope<T> | T;
  if (json !== null && typeof json === "object" && "success" in (json as ApiEnvelope<T>)) {
    const env = json as ApiEnvelope<T>;
    if (!env.success) throw buildEnvelopeError(env);
    return env.data as T;
  }
  return json as T;
}

async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  const headers: Record<string, string> = { Accept: "application/json" };
  let reqBody: BodyInit | undefined;
  if (body !== undefined && body !== null) {
    headers["Content-Type"] = "application/json";
    reqBody = JSON.stringify(body);
  }

  const res = await fetch(joinUrl(path), { method, headers, body: reqBody });
  if (!res.ok) {
    const text = await res.text();
    let parsed: unknown = null;
    try {
      parsed = text ? JSON.parse(text) : null;
    } catch {
      /* not JSON */
    }
    if (parsed && typeof parsed === "object" && ("errors" in parsed || "message" in parsed || "success" in parsed)) {
      throw buildEnvelopeError(parsed as { message?: string; errors?: string[] }, res.status);
    }
    throw new Error(text || `HTTP ${res.status}`);
  }

  const data = await parseResponse<T>(res);
  if (data === undefined) throw new Error("Empty API response");
  return data;
}

export const publicApi = {
  get: <T>(path: string) => request<T>("GET", path),
  post: <T>(path: string, body?: unknown) => request<T>("POST", path, body),
  put: <T>(path: string, body?: unknown) => request<T>("PUT", path, body),
};
