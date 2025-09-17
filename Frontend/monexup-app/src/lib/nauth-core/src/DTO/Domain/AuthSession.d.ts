import { LanguageEnum } from '../Enum/LanguageEnum';
export default interface AuthSession {
    userId: number;
    email: string;
    name: string;
    hash: string;
    token: string;
    isAdmin: boolean;
    language: LanguageEnum;
}
