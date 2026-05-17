window.artifactResize = {

    start: function (dotnet) {

        document.body.style.userSelect = "none";
        document.body.style.cursor = "ns-resize";

        function move(e) {

            const height =
                window.innerHeight - e.clientY;

            dotnet.invokeMethodAsync(
                'ResizeArtifactTable',
                height);
        }

        function up() {

            document.body.style.userSelect = "";
            document.body.style.cursor = "";

            window.removeEventListener(
                'mousemove', move);

            window.removeEventListener(
                'mouseup', up);
        }

        window.addEventListener(
            'mousemove', move);

        window.addEventListener(
            'mouseup', up);
    }
};