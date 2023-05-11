import { makeid, loadCss, loadJs, removeElements } from './loader.js';

const css = '//cdnjs.cloudflare.com/ajax/libs/swagger-ui/4.5.0/swagger-ui.css';
const js = '//cdnjs.cloudflare.com/ajax/libs/swagger-ui/4.5.0/swagger-ui-bundle.js';

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
 * @param {string} elementId
 * @param {string} backendUrl
 * @return Promise
 */
export async function render(elementId, backendUrl) {
    window.SwaggerUIBundle({
        url: `${backendUrl}api/v1/swagger.json`,
        dom_id: elementId,
        deepLinking: true,
        presets: [
            window.SwaggerUIBundle.presets.apis
        ],
        plugins: [
            window.SwaggerUIBundle.plugins.DownloadUrl
        ],
    });
}