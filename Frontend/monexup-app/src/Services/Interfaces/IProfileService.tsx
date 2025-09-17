import UserProfileInfo from "../../DTO/Domain/UserProfileInfo";
import StatusRequest from "../../DTO/Services/StatusRequest";
import UserProfileListResult from "../../DTO/Services/UserProfileListResult";
import UserProfileResult from "../../DTO/Services/UserProfileResult";
import IHttpClient from "../../Infra/Interface/IHttpClient";


export default interface IProfileService {
    init: (httpClient : IHttpClient) => void;
    insert: (profile: UserProfileInfo, token: string) => Promise<UserProfileResult>;
    update: (profile: UserProfileInfo, token: string) => Promise<UserProfileResult>;
    delete: (profileId: number, token: string) => Promise<StatusRequest>;
    listByNetwork: (networkId: number, token: string) => Promise<UserProfileListResult>;
    getById: (profileId: number, token: string) => Promise<UserProfileResult>;
}