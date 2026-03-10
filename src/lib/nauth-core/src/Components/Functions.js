import { jsxs as _jsxs, Fragment as _Fragment } from "react/jsx-runtime";
const showFrequencyMin = (frequency, t) => {
    switch (frequency) {
        case 0:
            return t('frequency_unique');
        case 7:
            return t('frequency_week');
        case 30:
            return t('frequency_month');
        case 60:
            return t('frequency_bimonthly');
        case 90:
            return t('frequency_quarter');
        case 180:
            return t('frequency_half');
        case 365:
            return t('frequency_year');
        default:
            return String(frequency);
    }
};
const showFrequencyMax = (frequency, t) => {
    switch (frequency) {
        case 0:
            return t('frequency_unique_payment');
        case 7:
            return t('frequency_weekly_payment');
        case 30:
            return t('frequency_monthly_payment');
        case 60:
            return t('frequency_bimonthly_payment');
        case 90:
            return t('frequency_quarterly_payment');
        case 180:
            return t('frequency_semiannual_payment');
        case 365:
            return t('frequency_annual_payment');
        default:
            return String(frequency);
    }
};
function formatPhoneNumber(phone) {
    // Remove qualquer caractere que não seja número
    const digits = phone.replace(/\D/g, '');
    if (digits.length !== 11) {
        return phone;
    } // Retorna original se não tiver 11 dígitos
    const ddd = digits.slice(0, 2);
    const firstDigit = digits.slice(2, 3);
    const firstPart = digits.slice(3, 7);
    const secondPart = digits.slice(7);
    return (_jsxs(_Fragment, { children: [_jsxs("small", { children: ["(", ddd, ")"] }), " ", firstDigit, " ", firstPart, "-", secondPart] }));
}
export { showFrequencyMin, showFrequencyMax, formatPhoneNumber };
