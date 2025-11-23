window.registerArrowKeys = function (dotNetObj) {
    document.addEventListener('keydown', function (e) {
        if (e.key === "ArrowRight") {
            dotNetObj.invokeMethodAsync('OnArrowRight');
        } else if (e.key === "ArrowLeft") {
            dotNetObj.invokeMethodAsync('OnArrowLeft');
        }
    });
};
