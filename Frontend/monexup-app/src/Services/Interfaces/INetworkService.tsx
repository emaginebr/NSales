import NetworkInfo from "../../DTO/Domain/NetworkInfo";
import NetworkInsertInfo from "../../DTO/Domain/NetworkInsertInfo";
import NetworkListResult from "../../DTO/Services/NetworkListResult";
import NetworkResult from "../../DTO/Services/NetworkResult";
import StatusRequest from "../../DTO/Services/StatusRequest";
import UserNetworkListResult from "../../DTO/Services/UserNetworkListResult";
import UserNetworkResult from "../../DTO/Services/UserNetworkResult";
import IHttpClient from "../../Infra/Interface/IHttpClient";


export default interface INetworkService {
    init: (httpClient : IHttpClient) => void;
    insert: (network: NetworkInsertInfo, token: string) => Promise<NetworkResult>;
    update: (network: NetworkInfo, token: string) => Promise<NetworkResult>;
    listAll: () => Promise<NetworkListResult>;
    listByUser: (token: string) => Promise<UserNetworkListResult>;
    listByNetwork: (networkSlug: string) => Promise<UserNetworkListResult>;
    getById: (networkId: number, token: string) => Promise<NetworkResult>;
    getBySlug: (networkSlug: string) => Promise<NetworkResult>;
    getUserNetwork: (networkId: number, token: string) => Promise<UserNetworkResult>;
    getUserNetworkBySlug: (networkSlug: string, token: string) => Promise<UserNetworkResult>;
    getSellerBySlug: (networkSlug: string, userSlug: string) => Promise<UserNetworkResult>;
    requestAccess: (networkId: number, token: string, referrerId?: number) => Promise<StatusRequest>;
    changeStatus: (networkId: number, userId: number, status: number, token: string) => Promise<StatusRequest>;
    promote: (networkId: number, userId: number, token: string) => Promise<StatusRequest>;
    demote: (networkId: number, userId: number, token: string) => Promise<StatusRequest>;
}