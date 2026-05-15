window.dragHelper = {
    startDrag: function (element, startX, startY) {

        event?.stopPropagation();

        let offsetX = startX;
        let offsetY = startY;

        function onMouseMove(e) {
                    const dx = e.clientX - offsetX;
        const dy = e.clientY - offsetY;

        const rect = element.getBoundingClientRect();

        element.style.left = (rect.left + dx) + "px";
        element.style.top = (rect.top + dy) + "px";

        element.style.bottom = "auto"; // wichtig!
        element.style.position = "fixed";

        offsetX = e.clientX;
        offsetY = e.clientY;
                }

        function onMouseUp() {
            document.removeEventListener("mousemove", onMouseMove);
        document.removeEventListener("mouseup", onMouseUp);
                }

        document.addEventListener("mousemove", onMouseMove);
        document.addEventListener("mouseup", onMouseUp);
    }
};
