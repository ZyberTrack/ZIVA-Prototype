
window.registerDragHandler = function (dotNetHelper) {
    const container = document.querySelector('.scroll-wrapper-timeline');
    if (!container) {
        console.warn('timeline-scrollable container not found!');
        return;
    }

    let isDragging = false;
    let startX = 0;
    let scrollLeft = 0;

    // Starten des Ziehens
    container.addEventListener('mousedown', function (e) {
        isDragging = true;
        startX = e.clientX; // Startpunkt der Maus
        scrollLeft = container.scrollLeft; // Aktuelle Scrollposition speichern
        container.style.cursor = 'grabbing'; // Mauszeiger auf "grabbing" setzen
    });

    // Stoppen des Ziehens, wenn die Maus den Container verlässt
    container.addEventListener('mouseleave', function () {
        if (isDragging) {
            isDragging = false; // Stoppen des Ziehens
            container.style.cursor = 'grab'; // Mauszeiger zurück auf "grab"
        }
    });

    // Stoppen des Ziehens, wenn die Maustaste losgelassen wird
    container.addEventListener('mouseup', function () {
        if (isDragging) {
            isDragging = false; // Stoppen des Ziehens
            container.style.cursor = 'grab'; // Mauszeiger zurück auf "grab"
        }
    });

    // Mausbewegung während des Ziehens
    container.addEventListener('mousemove', function (e) {
        if (!isDragging) return; // Nur ausführen, wenn der Benutzer zieht

        const diffX = e.clientX - startX; // Berechnung der Mausbewegung in X-Richtung
        container.scrollLeft = scrollLeft - diffX; // Zeitachse verschieben basierend auf der Mausbewegung
        dotNetHelper.invokeMethodAsync('OnScroll', diffX < 0 ? 'right' : 'left'); // Informiere Blazor über die Bewegung
    });

    container.addEventListener('mousedown', function (e) {

        // IGNORIERE Panel + alles darin
        if (e.target.closest('.entry-details')) return;

        isDragging = true;
        startX = e.clientX;
        scrollLeft = container.scrollLeft;
        container.style.cursor = 'grabbing';
    });
};
