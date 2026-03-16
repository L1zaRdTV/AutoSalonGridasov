document.addEventListener('DOMContentLoaded', () => {
    const collapseElement = document.getElementById('mainNav');
    if (collapseElement && window.bootstrap?.Collapse) {
        const navCollapse = bootstrap.Collapse.getOrCreateInstance(collapseElement, { toggle: false });

        collapseElement.querySelectorAll('a.nav-link').forEach(link => {
            link.addEventListener('click', () => {
                if (window.innerWidth < 992) {
                    navCollapse.hide();
                }
            });
        });
    }

    const revealElements = document.querySelectorAll('.ui-reveal');
    if ('IntersectionObserver' in window && revealElements.length) {
        const observer = new IntersectionObserver(entries => {
            entries.forEach((entry, index) => {
                if (!entry.isIntersecting) {
                    return;
                }

                const delay = Math.min(index * 70, 280);
                window.setTimeout(() => entry.target.classList.add('is-visible'), delay);
                observer.unobserve(entry.target);
            });
        }, { threshold: 0.15 });

        revealElements.forEach(element => observer.observe(element));
    } else {
        revealElements.forEach(element => element.classList.add('is-visible'));
    }

    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', () => {
            form.querySelectorAll('button[type="submit"]').forEach(button => {
                button.classList.add('disabled');
                button.setAttribute('aria-disabled', 'true');
            });
        });
    });
});
