window.scrollHelpers = {
    ignoreNextScroll: false,

    scrollTo: function (element, x, y) {
        if (!element) return;
        this.ignoreNextScroll = true;
        element.scrollTo({ left: x, top: y, behavior: 'auto' }); // oder 'smooth'
    }
};

window.timelineInterop = {
    registerScrollHandler: function (dotNetRef) {
        const scrollWrapper = document.querySelector('.scroll-wrapper-timeline');
        if (!scrollWrapper) return;

        // STRG + Scroll für Zoom
        scrollWrapper.addEventListener('wheel', function (e) {
            if (e.ctrlKey) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync("OnZoom", e.deltaY < 0 ? "in" : "out");
       
            } else {
                // normales Scrollen in horizontal umwandeln
                e.preventDefault(); // sonst scrollt Seite
                scrollWrapper.scrollLeft += e.deltaY;
            }
        }, { passive: false });

        // Scrollposition mit Schutz gegen "self-trigger"
        scrollWrapper.addEventListener('scroll', function () {
            if (window.scrollHelpers.ignoreNextScroll) {
                window.scrollHelpers.ignoreNextScroll = false;
                return; // nicht Blazor benachrichtigen
            }

            dotNetRef.invokeMethodAsync("OnAsyncScroll", scrollWrapper.scrollLeft);
        });
    }
};
