import UserAddressInfo from './UserAddressInfo';
import UserPhoneInfo from './UserPhoneInfo';
export default interface UserInfo {
    userId: number;
    email: string;
    slug: string;
    imageUrl: string;
    name: string;
    hash: string;
    password: string;
    isAdmin: boolean;
    birthDate: string;
    idDocument: string;
    pixKey: string;
    phones: UserPhoneInfo[];
    addresses: UserAddressInfo[];
    createAt: string;
    updateAt: string;
}
