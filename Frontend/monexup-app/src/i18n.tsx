import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import HttpApi from 'i18next-http-backend';
import { LanguageEnum } from './DTO/Enum/LanguageEnum';

// This utility function maps your LanguageEnum to i18next standard codes
// and helps select the correct flag.
export const getLangInfo = (lngCode: string | LanguageEnum) => {
  switch (lngCode) {
    case LanguageEnum.English:
    case 'en':
      return { code: 'en', flag: 'gb.svg', nameKey: 'language_english' };
    case LanguageEnum.Spanish:
    case 'es':
      return { code: 'es', flag: 'es.svg', nameKey: 'language_spanish' };
    case LanguageEnum.French:
    case 'fr':
      return { code: 'fr', flag: 'fr.svg', nameKey: 'language_french' };
    case LanguageEnum.Portuguese:
    case 'pt': // Using 'pt' as standard for Portuguese
    case 'br': // Allow 'br' from existing code to map to 'pt'
      return { code: 'pt', flag: 'br.svg', nameKey: 'language_portuguese' };
    default:
      return { code: 'en', flag: 'gb.svg', nameKey: 'language_english' }; // Default to English
  }
};

i18n
  .use(HttpApi) // Loads translations from /public/locales
  .use(LanguageDetector) // Detects user language
  .use(initReactI18next) // Passes i18n instance to react-i18next
  .init({
    supportedLngs: ['en', 'pt', 'es', 'fr'],
    fallbackLng: 'en',
    debug: process.env.NODE_ENV === 'development', // Enable debug output in development
    detection: {
      order: ['querystring', 'cookie', 'localStorage', 'sessionStorage', 'navigator', 'htmlTag'],
      caches: ['localStorage', 'cookie'],
    },
    backend: {
      loadPath: '/locales/{{lng}}/translation.json', // Path to translation files
    },
    interpolation: {
      escapeValue: false, // React already safes from xss
    },
  });

export default i18n;