window.timelineInterop = {
    registerScrollHandler: function (dotNetRef) {
        const scrollWrapper = document.querySelector('.scroll-wrapper-timeline');
        if (!scrollWrapper) return;

        // Wheel-Events für Zoom & Richtung
        scrollWrapper.addEventListener('wheel', function (e) {
            if (e.ctrlKey) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync("OnZoom", e.deltaY < 0 ? "in" : "out");
            } else {
                e.preventDefault();
                dotNetRef.invokeMethodAsync("OnScroll", e.deltaY < 0 ? "left" : "right");
            }
        }, { passive: false });

        // Scrollposition melden für Sichtbarkeitsprüfung
        scrollWrapper.addEventListener('scroll', function (e) {
            dotNetRef.invokeMethodAsync("OnScrollPositionChanged", scrollWrapper.scrollLeft);
        });
    }
};
