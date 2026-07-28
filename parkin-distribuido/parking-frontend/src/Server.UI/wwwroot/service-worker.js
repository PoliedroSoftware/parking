const CACHE_NAME = 'poliedro-v2';
const STATIC_ASSETS = [
  '/',
  '_framework/blazor.web.js',
  '_content/MudBlazor/MudBlazor.min.js',
  '_content/MudBlazor/MudBlazor.min.css',
  'css/app.css',
  'manifest.json',
  'icon-192.png',
  'logo_poliedro.png'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => {
      return cache.addAll(STATIC_ASSETS).catch(err => {
        console.warn('SW: some assets failed to cache, continuing', err);
      });
    })
  );
  self.skipWaiting();
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys => {
      return Promise.all(
        keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k))
      );
    })
  );
  self.clients.claim();
});

self.addEventListener('fetch', event => {
  if (!event.request.url.startsWith('http')) return;

  const url = new URL(event.request.url);

  if (event.request.method !== 'GET') return;

  if (url.pathname.startsWith('/_blazor') || url.pathname.startsWith('/signalRHub')) {
    return;
  }

  if (/\.(png|jpg|jpeg|gif|svg|ico|woff2?|ttf|eot)$/.test(url.pathname) ||
      /\.(css|js)$/.test(url.pathname)) {
    event.respondWith(
      caches.match(event.request).then(cached => {
        const fetchPromise = fetch(event.request).then(response => {
          if (response && response.status === 200) {
            const clone = response.clone();
            caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
          }
          return response;
        }).catch(() => cached);
        return cached || fetchPromise;
      })
    );
  }
});
