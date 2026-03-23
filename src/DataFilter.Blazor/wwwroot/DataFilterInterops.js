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

        handle.addEventListener('mousedown', function (e) {
            e.preventDefault();
            const startWidth = element.offsetWidth;
            const startHeight = element.offsetHeight;
            const startX = e.clientX;
            const startY = e.clientY;

            function doDrag(e) {
                element.style.width = (startWidth + e.clientX - startX) + 'px';
                element.style.height = (startHeight + e.clientY - startY) + 'px';
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
