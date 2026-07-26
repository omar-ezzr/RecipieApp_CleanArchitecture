export function resolveAssetUrl(
  path: string | null | undefined,
  apiBaseUrl: string
): string {
  if (!path) {
    return '/assets/recipe-placeholder.webp';
  }

  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  const assetBaseUrl = apiBaseUrl
    .replace(/\/$/, '')
    .replace(/\/api$/i, '');

  return `${assetBaseUrl}/${path.replace(/^\//, '')}`;
}
