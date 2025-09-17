import AuthSession from "../Domain/AuthSession";
import { LanguageEnum } from "../Enum/LanguageEnum";
import ProviderResult from "./ProviderResult";


interface IAuthProvider {
    loading: boolean;
    sessionInfo: AuthSession;
    language: LanguageEnum;
    setLanguage: (value: LanguageEnum) => void;
    //bindMetaMaskWallet: (name: string, email: string, fromReferralCode: string) => Promise<ProviderResult>;
    //checkUserRegister: () => Promise<ProviderResult>;
    setSession: (session: AuthSession) => void;
    loginWithEmail: (email: string, password: string) => Promise<ProviderResult>;
    logout: () => ProviderResult;
    loadUserSession: () => Promise<ProviderResult>;
    //updateUser: (name: string, email: string) => Promise<ProviderResult>;
}

export default IAuthProvider;