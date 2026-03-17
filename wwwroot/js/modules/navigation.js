export function initNavigationCollapse() {
    const collapseElement = document.getElementById('mainNav');
    if (!collapseElement || !window.bootstrap?.Collapse) {
        return;
    }

    const navCollapse = window.bootstrap.Collapse.getOrCreateInstance(collapseElement, { toggle: false });

    collapseElement.querySelectorAll('a.nav-link').forEach((link) => {
        link.addEventListener('click', () => {
            if (window.innerWidth < 992) {
                navCollapse.hide();
            }
        });
    });
}
