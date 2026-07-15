(() => {
    const maximumRetryCount = 3;
    const retryIntervalMilliseconds = 5000;
    const reconnectModal = document.getElementById('reconnect-modal');

    const startReconnectionProcess = () => {
        reconnectModal.style.display = 'flex';

        const statusEl = reconnectModal.querySelector('.reconnect-status');
        let isCanceled = false;

        (async () => {
            for (let i = 0; i < maximumRetryCount; i++) {
                if (statusEl) statusEl.textContent = `Reconectando... (${i + 1}/${maximumRetryCount})`;

                await new Promise(resolve => setTimeout(resolve, retryIntervalMilliseconds));

                if (isCanceled) {
                    return;
                }

                try {
                    const result = await Blazor.reconnect();
                    if (!result) {
                        location.reload();
                        return;
                    }
                    return;
                } catch {
                    // retry
                }
            }

            location.reload();
        })();

        return {
            cancel: () => {
                isCanceled = true;
                reconnectModal.style.display = 'none';
            },
        };
    };

    let currentReconnectionProcess = null;

    Blazor.start({
        reconnectionHandler: {
            onConnectionDown: () => currentReconnectionProcess ??= startReconnectionProcess(),
            onConnectionUp: () => {
                currentReconnectionProcess?.cancel();
                currentReconnectionProcess = null;
            },
        },
    });
})();