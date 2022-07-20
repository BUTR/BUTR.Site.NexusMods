import { BrotliDecode } from './decode.min.js';

export async function decode(compressedStreamReference) {
    const arrayBuffer = await compressedStreamReference.arrayBuffer();
    const int8array = new Uint8Array(arrayBuffer);
    return BrotliDecode(int8array);
}