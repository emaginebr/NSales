import BusinessResult from '../../DTO/Business/BusinessResult';
import AuthSession from '../../DTO/Domain/AuthSession';
import UserInfo from '../../DTO/Domain/UserInfo';
import UserTokenInfo from '../../DTO/Domain/UserTokenInfo';
import IUserService from '../../Services/Interfaces/IUserService';
export default interface IUserBusiness {
    init: (userService: IUserService) => void;
    getSession: () => AuthSession;
    setSession: (session: AuthSession) => void;
    cleanSession: () => void;
    uploadImageUser: (file: Blob) => Promise<BusinessResult<string>>;
    getMe: () => Promise<BusinessResult<UserInfo>>;
    getUserByEmail: (email: string) => Promise<BusinessResult<UserInfo>>;
    getBySlug: (slug: string) => Promise<BusinessResult<UserInfo>>;
    insert: (user: UserInfo) => Promise<BusinessResult<UserInfo>>;
    update: (user: UserInfo) => Promise<BusinessResult<UserInfo>>;
    loginWithEmail: (email: string, password: string) => Promise<BusinessResult<UserTokenInfo>>;
    hasPassword: () => Promise<BusinessResult<boolean>>;
    changePassword: (oldPassword: string, newPassword: string) => Promise<BusinessResult<boolean>>;
    sendRecoveryEmail: (email: string) => Promise<BusinessResult<boolean>>;
    changePasswordUsingHash: (recoveryHash: string, newPassword: string) => Promise<BusinessResult<boolean>>;
    list: (take: number) => Promise<BusinessResult<UserInfo[]>>;
}
