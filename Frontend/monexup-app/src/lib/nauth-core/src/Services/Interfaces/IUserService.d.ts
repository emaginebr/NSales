import UserInfo from '../../DTO/Domain/UserInfo';
import StatusRequest from '../../DTO/Services/StatusRequest';
import StringResult from '../../DTO/Services/StringResult';
import UserListResult from '../../DTO/Services/UserListResult';
import UserResult from '../../DTO/Services/UserResult';
import UserTokenResult from '../../DTO/Services/UserTokenResult';
import IHttpClient from '../../Infra/Interface/IHttpClient';
export default interface IUserService {
    init: (httpClient: IHttpClient) => void;
    uploadImageUser: (file: Blob, token: string) => Promise<StringResult>;
    getMe: (token: string) => Promise<UserResult>;
    getUserByEmail: (email: string) => Promise<UserResult>;
    getBySlug: (slug: string) => Promise<UserResult>;
    insert: (user: UserInfo) => Promise<UserResult>;
    update: (user: UserInfo, token: string) => Promise<UserResult>;
    loginWithEmail: (email: string, password: string) => Promise<UserTokenResult>;
    hasPassword: (token: string) => Promise<StatusRequest>;
    changePassword: (oldPassword: string, newPassword: string, token: string) => Promise<StatusRequest>;
    sendRecoveryEmail: (email: string) => Promise<StatusRequest>;
    changePasswordUsingHash: (recoveryHash: string, newPassword: string) => Promise<StatusRequest>;
    list: (take: number) => Promise<UserListResult>;
}
