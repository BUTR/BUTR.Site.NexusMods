import { BrotliDecode } from './decode.min.js';

/**
 * @param {Blob} compressedStreamReference
 * @return Promise<Blob>
 */
export async function decode(compressedStreamReference) {
    const arrayBuffer = await compressedStreamReference.arrayBuffer();
    const int8array = new Uint8Array(arrayBuffer);
    return BrotliDecode(int8array);
}