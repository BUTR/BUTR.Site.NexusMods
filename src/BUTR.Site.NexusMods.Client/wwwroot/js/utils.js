export async function downloadFile(filename, contentType, compressedStreamReference) {
    const arrayBuffer = await compressedStreamReference.arrayBuffer();
    const int8array = new Uint8Array(arrayBuffer);

    // Create the URL
    const file = new File([int8array], filename, { type: contentType });
    const exportUrl = URL.createObjectURL(file);

    // Create the <a> element and click on it
    const a = document.createElement('a');
    document.body.appendChild(a);
    a.href = exportUrl;
    a.download = filename;
    a.target = '_self';
    a.click();

    // We don't need to keep the object URL, let's release the memory
    // On older versions of Safari, it seems you need to comment this line...
    URL.revokeObjectURL(exportUrl);
}
