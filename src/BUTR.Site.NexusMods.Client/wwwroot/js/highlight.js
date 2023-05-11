import { makeid, loadCss, loadJs, removeElements } from './loader.js';

const css = '//cdn.jsdelivr.net/gh/highlightjs/cdn-release@11.6.0/build/styles/default.min.css';
const js = '//cdn.jsdelivr.net/gh/highlightjs/cdn-release@11.6.0/build/highlight.min.js';

let state = { };

/**
 * @param {DotNetObject} delegate
 * @return Promise
 */
export async function init(delegate) {
    state.scopeId = makeid(5);
    loadCss(css, state.scopeId);
    loadJs(js, state.scopeId, delegate);
}

/**
 * @return Promise
 */
export async function deinit() {
    removeElements(state.scopeId);
    state.scopeId = undefined;
}

/**
 * @param {Element} element
 * @return Promise
 */
export async function render(element) {
    if (element) {
        window.hljs.highlightElement(element);
    }
}