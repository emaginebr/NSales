import FingerprintJS from '@fingerprintjs/fingerprintjs';
let cachedFingerprint = null;
export const getFingerprint = async () => {
    if (cachedFingerprint)
        return cachedFingerprint;
    const fp = await FingerprintJS.load();
    const result = await fp.get();
    cachedFingerprint = result.visitorId;
    return cachedFingerprint;
};
