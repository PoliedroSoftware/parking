const { spawn } = require('node:child_process');
const fs = require('node:fs');
const path = require('node:path');
const { chromium, request: playwrightRequest } = require('playwright');

const root = path.resolve(__dirname, '..');
const artifacts = path.join(root, 'playwright-artifacts');

function start(cwd, args, env = {}) {
    const child = spawn('dotnet', args, {
        cwd,
        env: { ...process.env, ...env },
        windowsHide: true,
        stdio: ['ignore', 'pipe', 'pipe']
    });
    child.stdout.on('data', data => process.stdout.write(`[${path.basename(cwd)}] ${data}`));
    child.stderr.on('data', data => process.stderr.write(`[${path.basename(cwd)}] ${data}`));
    return child;
}

async function waitFor(url) {
    for (let attempt = 0; attempt < 60; attempt++) {
        try {
            const response = await fetch(url, { redirect: 'manual' });
            if (response.status > 0) return response.status;
        } catch { }
        await new Promise(resolve => setTimeout(resolve, 1000));
    }
    throw new Error(`No inició: ${url}`);
}

async function main() {
    fs.mkdirSync(artifacts, { recursive: true });
    const api = start(root, ['run', '--project', 'parking-api/src/Parking.Api', '--no-build', '--urls', 'http://localhost:5221'], {
        ASPNETCORE_ENVIRONMENT: 'Development',
        UseInMemoryDatabase: 'true'
    });
    const frontend = start(root, ['run', '--project', 'parking-frontend/src/Server.UI', '--no-build', '--urls', 'http://localhost:5057'], {
        ASPNETCORE_ENVIRONMENT: 'Development',
        UseInMemoryDatabase: 'true',
        ParkingApi__BaseUrl: 'http://localhost:5221'
    });
    const browser = await chromium.launch({ headless: true });

    try {
        const apiStatus = await waitFor('http://localhost:5221/swagger/index.html');
        const frontendStatus = await waitFor('http://localhost:5057/pages/tickets-api');
        const request = await playwrightRequest.newContext();
        const swagger = await request.get('http://localhost:5221/swagger/v1/swagger.json');
        console.log(JSON.stringify({ swaggerStatus: swagger.status(), vehicleTypeRoutes: Object.keys((await swagger.json()).paths).filter(path => path.includes('vehicle-types')) }));

        const login = await request.post('http://localhost:5221/api/v1/auth/login', {
            data: { username: 'Administrator', password: 'Password123!' }
        });
        if (!login.ok()) throw new Error(`Login API falló: ${login.status()}`);
        const token = (await login.json()).token;
        const headers = { Authorization: `Bearer ${token}` };
        const name = `Playwright-${Date.now()}`;

        const listBefore = await request.get('http://localhost:5221/api/v1/vehicle-types', { headers });
        console.log(JSON.stringify({ listBeforeStatus: listBefore.status(), listBeforeBody: (await listBefore.text()).slice(0, 300) }));
        const created = await request.post('http://localhost:5221/api/v1/vehicle-types', {
            headers,
            data: { name, icon: 'directions_car', isActive: true }
        });
        if (created.status() !== 200) throw new Error(`CREATE falló: ${created.status()} (GET inicial: ${listBefore.status()}) ${await created.text()}`);
        const id = (await created.json()).id;

        const updated = await request.put(`http://localhost:5221/api/v1/vehicle-types/${id}`, {
            headers,
            data: { name: `${name}-Updated`, icon: 'local_taxi', isActive: true }
        });
        if (updated.status() !== 200) throw new Error(`UPDATE falló: ${updated.status()}`);

        const listAfterUpdate = await request.get('http://localhost:5221/api/v1/vehicle-types', { headers });
        const updatedItems = await listAfterUpdate.json();
        if (!updatedItems.some(item => item.id === id && item.name === `${name}-Updated`))
            throw new Error('GET no encontró el registro actualizado');

        const deleted = await request.delete(`http://localhost:5221/api/v1/vehicle-types/${id}`, { headers });
        if (![200, 204].includes(deleted.status())) throw new Error(`DELETE falló: ${deleted.status()}`);

        const listAfterDelete = await request.get('http://localhost:5221/api/v1/vehicle-types', { headers });
        if ((await listAfterDelete.json()).some(item => item.id === id))
            throw new Error('DELETE no removió el registro');

        const page = await browser.newPage({ viewport: { width: 1440, height: 1000 } });
        await page.goto('http://localhost:5057/account/login', { waitUntil: 'networkidle' });
        console.log(JSON.stringify({ loginUrl: page.url(), loginTitle: await page.title(), inputCount: await page.locator('input').count(), loginBody: (await page.locator('body').innerText()).replace(/\s+/g, ' ').trim().slice(0, 240) }));
        const usernameInput = page.locator('input[type="text"]').first();
        const passwordInput = page.locator('input[type="password"]').first();
        await usernameInput.waitFor({ state: 'visible', timeout: 15000 });
        await usernameInput.fill('Administrator');
        await passwordInput.fill('Password123!');
        await page.locator('button[type="submit"]').click();
        await page.waitForTimeout(1500);
        await page.goto('http://localhost:5057/pages/tickets-api', { waitUntil: 'networkidle' });
        await page.getByRole('button', { name: 'Iniciar sesión API' }).waitFor({ state: 'visible' });
        await page.screenshot({ path: path.join(artifacts, 'crud-frontend.png'), fullPage: true });

        console.log(JSON.stringify({
            apiStatus,
            frontendStatus,
            crud: { listBefore: listBefore.status(), create: created.status(), update: updated.status(), delete: deleted.status() },
            frontendUrl: page.url(),
            screenshot: path.join(artifacts, 'crud-frontend.png')
        }, null, 2));
    } finally {
        await browser.close();
        api.kill();
        frontend.kill();
    }
}

main().catch(error => {
    console.error(error);
    process.exitCode = 1;
});
