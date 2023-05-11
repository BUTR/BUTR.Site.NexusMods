import { makeid, loadCss, loadJs, removeElements } from './loader.js';

const cssGitHub = '//cdnjs.cloudflare.com/ajax/libs/highlight.js/9.13.1/styles/github.min.css';
const css = '//cdn.jsdelivr.net/npm/diff2html@3.4.35/bundles/css/diff2html.min.css';
const js = '//cdn.jsdelivr.net/npm/diff2html@3.4.35/bundles/js/diff2html-ui.min.js';
//const js = '../js/diff2html-ui-slim.min.js';

let state = { };

/**
 * @param {DotNetObject} delegate
 * @return Promise
 */
export async function init(delegate) {
    state.scopeId = makeid(5);
    loadCss(cssGitHub, state.scopeId);
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
 * @param {string} elementId
 * @param {string} diffString
 * @param {Object} configuration
 * @return Promise
 */
export async function render(elementId, diffString, configuration) {
    const targetElement = document.getElementById(elementId);
    if (targetElement) {
        const diff2htmlUi = new window.Diff2HtmlUI(targetElement, diffString, configuration);
        diff2htmlUi.draw();
        diff2htmlUi.highlightCode();
    }
}