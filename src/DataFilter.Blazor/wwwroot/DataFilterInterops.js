window.DataFilterInterops = {
    getAnchoredPopupPosition: function (buttonId, popupId, margin) {
        const btn = document.getElementById(buttonId);
        const pop = document.getElementById(popupId);
        if (!btn || !pop) return null;

        const btnRect = btn.getBoundingClientRect();
        const popRect = pop.getBoundingClientRect();
        const m = (typeof margin === 'number') ? margin : 8;
        const dir = (getComputedStyle(btn).direction || 'ltr').toLowerCase();

        // Default anchor rule:
        // - LTR: popup top-left at button bottom-right
        // - RTL: popup top-right at button bottom-left
        let left = (dir === 'rtl') ? (btnRect.left - popRect.width) : btnRect.right;
        let top = btnRect.bottom;

        const maxLeft = Math.max(m, window.innerWidth - popRect.width - m);
        const maxTop = Math.max(m, window.innerHeight - popRect.height - m);

        left = Math.min(Math.max(left, m), maxLeft);
        top = Math.min(Math.max(top, m), maxTop);

        return { top: top, left: left };
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
