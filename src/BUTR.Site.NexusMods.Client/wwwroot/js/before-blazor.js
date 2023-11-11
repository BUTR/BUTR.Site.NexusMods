/**
 * @param {string} id
 * @return void
 */
function BlazorScrollToId(id) {
    const element = document.getElementById(id);
    if (element instanceof HTMLElement) {
        const y = element.getBoundingClientRect().top + window.scrollY + -80;
        window.scrollTo({ top: y, behavior: "smooth" });
    }
}