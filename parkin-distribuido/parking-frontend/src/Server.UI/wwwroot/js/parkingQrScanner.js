let stream;
let animationFrame;
let detector;
let dotNetReference;

export async function startParkingQrScanner(videoId, reference) {
    if (!('BarcodeDetector' in window))
        throw new Error('Este navegador no soporta lectura QR nativa.');

    detector = new BarcodeDetector({ formats: ['qr_code'] });
    dotNetReference = reference;
    const video = document.getElementById(videoId);
    if (!video)
        throw new Error('No se encontró el visor de la cámara.');

    stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: { ideal: 'environment' } } });
    video.srcObject = stream;
    await video.play();
    scan(video);
}

async function scan(video) {
    if (!stream || !detector)
        return;

    try {
        const codes = await detector.detect(video);
        if (codes.length > 0 && codes[0].rawValue) {
            await dotNetReference.invokeMethodAsync('OnParkingQrDetected', codes[0].rawValue);
            return;
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
