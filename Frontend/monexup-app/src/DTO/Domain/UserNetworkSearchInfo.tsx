import { UserNetworkStatusEnum } from "../Enum/UserNetworkStatusEnum";
import { UserRoleEnum } from "../Enum/UserRoleEnum";

export default interface UserNetworkSearchInfo {
    userId: number;
    networkId: number;
    profileId?: number;
    name: string;
    email: string;
    profile: string;
    level: number;
    commission: number;
    role: UserRoleEnum;
    status: UserNetworkStatusEnum;
}