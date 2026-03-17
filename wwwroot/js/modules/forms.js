export function initSubmitProtection() {
    document.querySelectorAll('form').forEach((form) => {
        form.addEventListener('submit', () => {
            form.querySelectorAll('button[type="submit"]').forEach((button) => {
                button.classList.add('disabled');
                button.setAttribute('aria-disabled', 'true');
            });
        });
    });
}
