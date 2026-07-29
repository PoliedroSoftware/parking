(() => {
    let deferredPrompt = null;

    const isStandalone = () =>
        window.matchMedia('(display-mode: standalone)').matches ||
        window.navigator.standalone === true;

    const isMobileViewport = () =>
        window.matchMedia('(max-width: 768px)').matches || navigator.maxTouchPoints > 0;

    const setVisible = (banner, visible) => {
        banner.hidden = !visible;
        banner.setAttribute('aria-hidden', visible ? 'false' : 'true');
    };

    const showBrowserInstructions = help => {
        help.textContent = /iphone|ipad|ipod/i.test(navigator.userAgent)
            ? 'En Safari, usa Compartir y luego Agregar a inicio.'
            : 'Abre el menu del navegador y selecciona Instalar app o Agregar a pantalla de inicio.';
    };

    window.parkingPwaInstall = {
        init() {
            const banner = document.getElementById('pwa-install-banner');
            const button = document.getElementById('pwa-install-button');
            const help = document.getElementById('pwa-install-help');

            if (!banner || !button || !help || isStandalone() || !isMobileViewport()) return;

            setVisible(banner, true);
            showBrowserInstructions(help);

            window.addEventListener('beforeinstallprompt', event => {
                event.preventDefault();
                deferredPrompt = event;
                help.textContent = 'Instala la aplicacion en este dispositivo.';
            });

            window.addEventListener('appinstalled', () => {
                deferredPrompt = null;
                setVisible(banner, false);
            });

            button.addEventListener('click', async () => {
                if (!deferredPrompt) {
                    showBrowserInstructions(help);
                    return;
                }

                deferredPrompt.prompt();
                const choice = await deferredPrompt.userChoice;
                if (choice.outcome === 'accepted') setVisible(banner, false);
                deferredPrompt = null;
            });
        }
    };

    window.addEventListener('load', () => window.parkingPwaInstall.init());
})();
