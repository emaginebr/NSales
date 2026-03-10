import NetworkInfo from "../Domain/NetworkInfo";
import NetworkInsertInfo from "../Domain/NetworkInsertInfo";
import UserNetworkInfo from "../Domain/UserNetworkInfo";
import { UserRoleEnum } from "../Enum/UserRoleEnum";
import NetworkProviderResult from "./NetworkProviderResult";
import ProviderResult from "./ProviderResult";


interface INetworkProvider {
    loading: boolean;
    loadingTeam: boolean;
    loadingSeller: boolean;
    loadingUpdate: boolean;
    loadingRequestAccess: boolean;

    network: NetworkInfo;
    networks: NetworkInfo[];
    userNetwork: UserNetworkInfo;
    seller: UserNetworkInfo;
    userNetworks: UserNetworkInfo[];
    teams: UserNetworkInfo[];
    currentRole: UserRoleEnum;

    setNetwork: (network: NetworkInfo) => void;
    setUserNetwork: (userNetwork: UserNetworkInfo) => void;
    setCurrentRole: (role: UserRoleEnum) => void;

    insert: (network: NetworkInsertInfo) => Promise<ProviderResult>;
    update: (network: NetworkInfo) => Promise<ProviderResult>;
    listAll: () => Promise<ProviderResult>;
    listByUser: () => Promise<ProviderResult>;
    listByNetwork: (networkSlug: string) => Promise<ProviderResult>;
    getById: (networkId: number) => Promise<ProviderResult>;
    getBySlug: (networkSlug: string) => Promise<NetworkProviderResult>;
    getUserNetwork: (networkId: number) => Promise<ProviderResult>;
    getUserNetworkBySlug: (networkSlug: string) => Promise<ProviderResult>;
    getSellerBySlug: (networkSlug: string, userSlug: string) => Promise<ProviderResult>;
    requestAccess: (networkId: number, referrerId?: number) => Promise<ProviderResult>;
    changeStatus: (networkId: number, userId: number, status: number) => Promise<ProviderResult>;
    promote: (networkId: number, userId: number) => Promise<ProviderResult>;
    demote: (networkId: number, userId: number) => Promise<ProviderResult>;
}

export default INetworkProvider;