// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.// Wacht tot de pagina geladen is
document.addEventListener("DOMContentLoaded", function () {

    // 1. Navbar Scroll Effect
    const navbar = document.querySelector('.navbar');

    if (navbar) {
        window.addEventListener('scroll', () => {
            if (window.scrollY > 50) {
                navbar.classList.add('scrolled');
                navbar.style.padding = '0.5rem 1.5rem'; // Kleiner maken
                navbar.style.opacity = '0.95'; // Iets doorzichtig
            } else {
                navbar.classList.remove('scrolled');
                navbar.style.padding = '1rem 1.5rem'; // Normale grootte
                navbar.style.opacity = '1';
            }
        });
    }

    // 2. Auto-hide Alerts (Succes/Error berichten)
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        // Wacht 4 seconden en fade dan uit
        setTimeout(() => {
            alert.style.transition = "opacity 0.5s ease";
            alert.style.opacity = "0";
            // Na de animatie, verwijder uit de DOM
            setTimeout(() => alert.remove(), 500);
        }, 4000);
    });

    // 3. Scroll Reveal Animation (Elementen komen mooi binnen als je scrollt)
    const observerOptions = {
        threshold: 0.1
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Voeg de 'fade-in-section' class toe aan alle cards en forms dynamisch
    document.querySelectorAll('.card, form, .table').forEach(el => {
        el.classList.add('fade-in-section');
        observer.observe(el);
    });
});