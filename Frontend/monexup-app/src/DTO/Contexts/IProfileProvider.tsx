import UserProfileInfo from "../Domain/UserProfileInfo";
import ProviderResult from "./ProviderResult";


interface IProfileProvider {
    loading: boolean;
    loadingUpdate: boolean;

    profile: UserProfileInfo;
    profiles: UserProfileInfo[];

    setProfile: (profile: UserProfileInfo) => void;
    
    insert: (profile: UserProfileInfo) => Promise<ProviderResult>;
    update: (profile: UserProfileInfo) => Promise<ProviderResult>;
    delete: (profileId: number) => Promise<ProviderResult>;
    listByNetwork: (networkId: number) => Promise<ProviderResult>;
    getById: (profileId: number) => Promise<ProviderResult>;
}

export default IProfileProvider;