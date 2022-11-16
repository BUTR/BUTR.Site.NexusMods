import { makeid, loadCss, loadJs, removeElements } from './loader.js';

const cssGitHub = '//cdnjs.cloudflare.com/ajax/libs/highlight.js/9.13.1/styles/github.min.css';
const css = '//cdn.jsdelivr.net/npm/diff2html/bundles/css/diff2html.min.css';
const js = '//cdn.jsdelivr.net/npm/diff2html/bundles/js/diff2html-ui-slim.min.js';

let state = { };

export async function init(delegate) {
    state.scopeId = makeid(5);
    loadCss(cssGitHub, state.scopeId);
    loadCss(css, state.scopeId);
    loadJs(js, state.scopeId, delegate);
}

export async function deinit() {
    removeElements(state.scopeId);
    state.scopeId = undefined;
}

export async function render(elementId, diffString, configuration) {
    const targetElement = document.getElementById(elementId);
    if (targetElement) {
        const diff2htmlUi = new window.Diff2HtmlUI(targetElement, diffString, configuration);
        diff2htmlUi.draw();
        diff2htmlUi.highlightCode();
    }
}