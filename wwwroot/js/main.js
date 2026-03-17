import { initNavigationCollapse } from './modules/navigation.js';
import { initRevealAnimations } from './modules/reveal.js';
import { initSubmitProtection } from './modules/forms.js';
import { initCarsRegistry } from './modules/carData.js';

document.addEventListener('DOMContentLoaded', () => {
    initCarsRegistry();
    initNavigationCollapse();
    initRevealAnimations();
    initSubmitProtection();
});
