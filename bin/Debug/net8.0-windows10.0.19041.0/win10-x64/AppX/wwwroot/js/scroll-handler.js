window.registerScrollHandler = function (dotNetRef) {
    const scrollWrapper = document.querySelector('.scroll-wrapper-timeline');

    if (!scrollWrapper) return;

    scrollWrapper.addEventListener('wheel', function (e) {
        if (e.ctrlKey) {
            // Zoom statt Scroll
            e.preventDefault(); // Verhindert normales Scrollen
            dotNetRef.invokeMethodAsync("OnZoom", e.deltaY < 0 ? "in" : "out");
        } else {
            // Nur scrollen, wenn STRG nicht gedrückt
            e.preventDefault();
            dotNetRef.invokeMethodAsync("OnScroll", e.deltaY < 0 ? "left" : "right");
        }
    }, { passive: false });
};
