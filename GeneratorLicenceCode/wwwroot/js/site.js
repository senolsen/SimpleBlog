function copyLicenseKey() {
    const keyElement = document.getElementById('licenseKey');
    if (!keyElement) return;

    const text = keyElement.textContent.trim();
    navigator.clipboard.writeText(text).then(() => {
        const btn = event.target;
        const originalText = btn.textContent;
        btn.textContent = 'Kopyalandı!';
        setTimeout(() => { btn.textContent = originalText; }, 2000);
    });
}
