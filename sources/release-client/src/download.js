/**
 * Lets the browser save a blob as file.
 */
export function saveBlob (blob, fileName) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  // Give the browser time to start the download before the URL is released
  setTimeout(() => URL.revokeObjectURL(url), 10000);
}
