import { makeid, loadCss, loadJs, removeElements } from './loader.js';

const css = 'css/prismjs-custom.css';
const js = 'js/prismjs-custom.js';

let state = { };

/**
 * @param {string} baseUri
 * @param {DotNetObject} delegate
 * @return Promise
 */
export async function init(baseUri, delegate) {
    window.Prism = window.Prism || { };
    window.Prism.manual = true;
    
    state.scopeId = makeid(5);
    loadCss(`${baseUri}${css}`, state.scopeId);
    loadJs(`${baseUri}${js}`, state.scopeId, delegate);
}

/**
 * @return Promise
 */
export async function deinit() {
    removeElements(state.scopeId);
    state.scopeId = undefined;
}

/**
 * @return Promise
 */
export async function highlightAll() {
    return window.Prism.highlightAll();
}

/**
 * @param {string} code
 * @return Promise
 */
export async function highlightCIL(code) {
    return window.Prism.highlight(code, Prism.languages.cil, 'cil');
}