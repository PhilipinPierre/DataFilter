window.DataFilterInterops = {
    getPosition: function (elementId) {
        var el = document.getElementById(elementId);
        if (el) {
            var rect = el.getBoundingClientRect();
            return {
                top: rect.bottom,
                left: rect.left
            };
        }
        return null;
    },
    addOutsideClickListener: function (dotnetHelper, elementId, toggleButtonId) {
        const listener = function (e) {
            var el = document.getElementById(elementId);
            var btn = document.getElementById(toggleButtonId);
            if (el && !el.contains(e.target) && (!btn || !btn.contains(e.target))) {
                dotnetHelper.invokeMethodAsync('ClosePopup');
                window.removeEventListener('mousedown', listener);
            }
        };
        setTimeout(() => window.addEventListener('mousedown', listener), 10);
    },
    initResizable: function (elementId, handleId) {
        const element = document.getElementById(elementId);
        const handle = document.getElementById(handleId);
        if (!element || !handle) return;

        const margin = 8;

        function clampToViewport() {
            const rect = element.getBoundingClientRect();
            const availableHeight = Math.max(0, window.innerHeight - rect.top - margin);
            const availableWidth = Math.max(0, window.innerWidth - rect.left - margin);

            // Keep popup within viewport so it isn't visually "cut off".
            element.style.maxHeight = availableHeight + 'px';
            element.style.maxWidth = availableWidth + 'px';
        }

        clampToViewport();

        handle.addEventListener('mousedown', function (e) {
            e.preventDefault();
            const startWidth = element.offsetWidth;
            const startHeight = element.offsetHeight;
            const startX = e.clientX;
            const startY = e.clientY;

            function doDrag(e) {
                const nextWidth = (startWidth + e.clientX - startX);
                const nextHeight = (startHeight + e.clientY - startY);

                // Recompute viewport bounds so clamping stays correct while dragging.
                const rect = element.getBoundingClientRect();
                const availableHeight = Math.max(0, window.innerHeight - rect.top - margin);
                const availableWidth = Math.max(0, window.innerWidth - rect.left - margin);

                element.style.width = Math.min(nextWidth, availableWidth) + 'px';
                element.style.height = Math.min(nextHeight, availableHeight) + 'px';
            }

            function stopDrag() {
                document.removeEventListener('mousemove', doDrag);
                document.removeEventListener('mouseup', stopDrag);
            }

            document.addEventListener('mousemove', doDrag);
            document.addEventListener('mouseup', stopDrag);
        });
    }
};
