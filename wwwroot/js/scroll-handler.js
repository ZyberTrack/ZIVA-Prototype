window.registerScrollHandler = function (dotNetHelper) {
    const container = document.querySelector('.scroll-wrapper-timeline');
    if (!container) {
        console.warn('timeline-scrollable container not found!');
        return;
    }

    container.addEventListener('wheel', function (e) {
        if (e.deltaY === 0) return;

        e.preventDefault(); // Damit nicht vertikal gescrollt wird

        // Scroll horizontal um den deltaY-Wert
        container.scrollLeft += e.deltaY;

        const direction = e.deltaY > 0 ? 'right' : 'left';
        dotNetHelper.invokeMethodAsync('OnScroll', direction);
    }, { passive: false }); // Wichtig für preventDefault()
};
