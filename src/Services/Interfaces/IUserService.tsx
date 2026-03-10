import UserInfo from "../../DTO/Domain/UserInfo";
import StatusRequest from "../../DTO/Services/StatusRequest";
import UserListPagedResult from "../../DTO/Services/UserListPagedResult";
import UserListResult from "../../DTO/Services/UserListResult";
import UserResult from "../../DTO/Services/UserResult";
import UserTokenResult from "../../DTO/Services/UserTokenResult";
import IHttpClient from "../../Infra/Interface/IHttpClient";


export default interface IUserService {
    init: (httpClient : IHttpClient) => void;
    getMe: (token: string) => Promise<UserResult>;
    getUserByEmail: (email: string) => Promise<UserResult>;
    getBySlug: (slug: string) => Promise<UserResult>;
    getTokenAuthorized: (email: string, password: string) => Promise<UserTokenResult>;
    insert: (user: UserInfo) => Promise<UserResult>;
    update: (user: UserInfo, token: string) => Promise<UserResult>;
    loginWithEmail: (email: string, password: string) => Promise<UserResult>;
    hasPassword: (token: string) => Promise<StatusRequest>;
    changePassword: (oldPassword: string, newPassword: string, token: string) => Promise<StatusRequest>;
    sendRecoveryEmail: (email: string) => Promise<StatusRequest>;
    changePasswordUsingHash: (recoveryHash: string, newPassword: string) => Promise<StatusRequest>; 
    list: (take: number) => Promise<UserListResult>;
    search: (networkId: number, keyword: string, pageNum: number, token: string, profileId?: number) => Promise<UserListPagedResult>;
}