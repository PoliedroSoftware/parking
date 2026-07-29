let stream;
let animationFrame;
let detector;
let dotNetReference;
let canvas;
let context;

export async function startParkingQrScanner(videoId, reference) {
    detector = 'BarcodeDetector' in window
        ? new BarcodeDetector({ formats: ['qr_code'] })
        : null;
    dotNetReference = reference;
    if (!detector && typeof window.jsQR !== 'function')
        throw new Error('Este navegador no tiene un lector QR disponible.');
    const video = document.getElementById(videoId);
    if (!video)
        throw new Error('No se encontró el visor de la cámara.');

    stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: { ideal: 'environment' } } });
    video.srcObject = stream;
    await video.play();
    scan(video);
}

async function scan(video) {
    if (!stream || (!detector && typeof window.jsQR !== 'function'))
        return;

    try {
        if (detector) {
            const codes = await detector.detect(video);
            if (codes.length > 0 && codes[0].rawValue) {
                await dotNetReference.invokeMethodAsync('OnParkingQrDetected', codes[0].rawValue);
                return;
            }
        } else if (video.readyState >= HTMLMediaElement.HAVE_CURRENT_DATA && video.videoWidth > 0) {
            canvas ??= document.createElement('canvas');
            context ??= canvas.getContext('2d', { willReadFrequently: true });
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            context.drawImage(video, 0, 0, canvas.width, canvas.height);
            const image = context.getImageData(0, 0, canvas.width, canvas.height);
            const code = window.jsQR(image.data, image.width, image.height, { inversionAttempts: 'attemptBoth' });
            if (code?.data) {
                await dotNetReference.invokeMethodAsync('OnParkingQrDetected', code.data);
                return;
            }
        }
    } catch { }

    animationFrame = requestAnimationFrame(() => scan(video));
}

export function stopParkingQrScanner() {
    if (animationFrame)
        cancelAnimationFrame(animationFrame);
    if (stream)
        stream.getTracks().forEach(track => track.stop());

    stream = undefined;
    detector = undefined;
    dotNetReference = undefined;
}
