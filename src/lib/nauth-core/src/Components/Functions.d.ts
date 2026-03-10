declare const showFrequencyMin: (frequency: number, t: (key: string) => string) => string;
declare const showFrequencyMax: (frequency: number, t: (key: string) => string) => string;
declare function formatPhoneNumber(phone: string): string | import("react/jsx-runtime").JSX.Element;
export { showFrequencyMin, showFrequencyMax, formatPhoneNumber };
