window.DataFilterInterops = (function () {
    const active = new Map(); // popupId -> { handler, rafId, lastTop, lastLeft }

    function getAnchoredPopupPosition(buttonId, popupId, margin) {
        const btn = document.getElementById(buttonId);
        const pop = document.getElementById(popupId);
        if (!btn || !pop) return null;

        const btnRect = btn.getBoundingClientRect();
        const popRect = pop.getBoundingClientRect();
        const m = (typeof margin === 'number') ? margin : 8;
        const dir = (getComputedStyle(btn).direction || 'ltr').toLowerCase();

        let left = (dir === 'rtl') ? (btnRect.left - popRect.width) : btnRect.right;
        let top = btnRect.bottom;

        const maxLeft = Math.max(m, window.innerWidth - popRect.width - m);
        const maxTop = Math.max(m, window.innerHeight - popRect.height - m);

        left = Math.min(Math.max(left, m), maxLeft);
        top = Math.min(Math.max(top, m), maxTop);

        return { top: top, left: left };
    }

    function setAnchoredPopupPosition(buttonId, popupId, margin) {
        const pos = getAnchoredPopupPosition(buttonId, popupId, margin);
        const pop = document.getElementById(popupId);
        if (!pos || !pop) return false;
        pop.style.top = pos.top + 'px';
        pop.style.left = pos.left + 'px';
        return true;
    }

    function startAnchoredPopupAutoUpdate(buttonId, popupId, margin) {
        if (!popupId || active.has(popupId)) return;

        const handler = function () {
            // Single-shot update (used by scroll/resize triggers)
            setAnchoredPopupPosition(buttonId, popupId, margin);
        };

        window.addEventListener('scroll', handler, true);
        window.addEventListener('resize', handler, true);

        const entry = { handler: handler, rafId: 0, lastTop: null, lastLeft: null };
        active.set(popupId, entry);

        // rAF loop is the most reliable way to stay anchored across all scrolling containers
        // (scroll events can be missed depending on the element / platform).
        const loop = function () {
            const e = active.get(popupId);
            if (!e) return;

            const pos = getAnchoredPopupPosition(buttonId, popupId, margin);
            const pop = document.getElementById(popupId);
            if (pos && pop) {
                // Only touch the DOM when coordinates actually changed.
                if (e.lastTop !== pos.top || e.lastLeft !== pos.left) {
                    e.lastTop = pos.top;
                    e.lastLeft = pos.left;
                    pop.style.top = pos.top + 'px';
                    pop.style.left = pos.left + 'px';
                }
            }

            e.rafId = window.requestAnimationFrame(loop);
        };

        handler(); // initial
        entry.rafId = window.requestAnimationFrame(loop);
    }

    function stopAnchoredPopupAutoUpdate(popupId) {
        if (!popupId) return;
        const entry = active.get(popupId);
        if (!entry) return;

        window.removeEventListener('scroll', entry.handler, true);
        window.removeEventListener('resize', entry.handler, true);
        if (entry.rafId) window.cancelAnimationFrame(entry.rafId);
        active.delete(popupId);
    }

    function addOutsideClickListener(dotnetHelper, elementId, toggleButtonId, closeMethodName) {
        const listener = function (e) {
            var el = document.getElementById(elementId);
            var btn = document.getElementById(toggleButtonId);
            if (el && !el.contains(e.target) && (!btn || !btn.contains(e.target))) {
                dotnetHelper.invokeMethodAsync(closeMethodName || 'ClosePopup');
                window.removeEventListener('mousedown', listener);
            }
        };
        setTimeout(() => window.addEventListener('mousedown', listener), 10);
    }

    function initResizable(elementId, handleId) {
        const element = document.getElementById(elementId);
        const handle = document.getElementById(handleId);
        if (!element || !handle) return;

        const margin = 8;

        function clampToViewport() {
            const rect = element.getBoundingClientRect();
            const availableHeight = Math.max(0, window.innerHeight - rect.top - margin);
            const availableWidth = Math.max(0, window.innerWidth - rect.left - margin);

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

    return {
        getAnchoredPopupPosition: getAnchoredPopupPosition,
        setAnchoredPopupPosition: setAnchoredPopupPosition,
        startAnchoredPopupAutoUpdate: startAnchoredPopupAutoUpdate,
        stopAnchoredPopupAutoUpdate: stopAnchoredPopupAutoUpdate,
        addOutsideClickListener: addOutsideClickListener,
        initResizable: initResizable
    };
})();
