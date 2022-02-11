const css = '//cdnjs.cloudflare.com/ajax/libs/swagger-ui/4.5.0/swagger-ui.css';
const js = '//cdnjs.cloudflare.com/ajax/libs/swagger-ui/4.5.0/swagger-ui-es-bundle.js';

Object.defineProperty(HTMLElement.prototype, 'setAttribute2', {
    value: function(name, value) {
        this.setAttribute(name, value);
        return this;
    },
    writable: true,
    configurable: true
});

// I lack the expertise to make it normally
async function importModule(url) {
    const originalModule = window.module;
    const module = { exports: { } };
    window.module = module;
    // ReSharper disable once UseOfImplicitGlobalInFunctionScope
    await import(url);
    window.module = originalModule;
    return module.exports;
}
// I lack the expertise to make it normally

function makeid(length) {
    let result = '';
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz';
    const charactersLength = characters.length;
    for (let i = 0; i < length; i++) {
        result += characters.charAt(Math.floor(Math.random() * charactersLength));
    }
    return result;
}

function isEmptyObject(obj) {
    for (let name in obj) {
        if (obj.hasOwnProperty(name)) {
            return false;
        }
    }
    return true;
}


let state = { };

function addCss() {
    const element = document.createElement('link')
        .setAttribute2('id', state.scopeId)
        .setAttribute2('rel', 'stylesheet')
        .setAttribute2('type', 'text/css')
        .setAttribute2('href', css);
    document.getElementsByTagName('head')[0].appendChild(element);
}
function removeCss() {
    document.getElementById(state.scopeId).remove();
}

async function render(elementId, backendUrl) {
    const importResult = await importModule(js);
    if (importResult && !isEmptyObject(importResult))
        state.SwaggerUIBundle = importResult;

    state.SwaggerUIBundle({
        url: `${backendUrl}api/v1/swagger.json`,
        dom_id: elementId,
        deepLinking: true,
        presets: [
            state.SwaggerUIBundle.presets.apis
        ],
        plugins: [
            state.SwaggerUIBundle.plugins.DownloadUrl
        ],
    });
}

export async function init(elementId, backendUrl) {
    state.scopeId = makeid(5);
    addCss();
    await render(elementId, backendUrl);
}

export async function deinit() {
    removeCss();
    state.scopeId = undefined;
}