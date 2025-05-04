window.registerZoomHandler = (dotnetHelper) => {
    window.addEventListener('wheel', function (e) {
        if (e.ctrlKey) {
            e.preventDefault(); // verhindert Browser-Zoom
            const direction = e.deltaY < 0 ? "in" : "out";
            dotnetHelper.invokeMethodAsync('OnZoom', direction);
        }
    }, { passive: false });
};