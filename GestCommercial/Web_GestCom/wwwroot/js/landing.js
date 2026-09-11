// Scroll-reveal for the public landing page (Home.razor, anonymous branch).
// Blazor Server does not execute <script> tags inserted via its own render diffing,
// so this lives in a real static file loaded from App.razor and is triggered via
// IJSRuntime from OnAfterRenderAsync instead.
window.gcInitLandingReveal = function () {
    var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var items = document.querySelectorAll('.lp .reveal');

    if (reduceMotion || !('IntersectionObserver' in window)) {
        items.forEach(function (el) { el.classList.add('is-visible'); });
        return;
    }

    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add('is-visible');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12 });

    items.forEach(function (el) { observer.observe(el); });
};
