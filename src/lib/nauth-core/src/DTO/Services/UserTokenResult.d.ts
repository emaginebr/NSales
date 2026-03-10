import UserInfo from '../Domain/UserInfo';
import StatusRequest from './StatusRequest';
export default interface UserTokenResult extends StatusRequest {
    token?: string;
    user?: UserInfo;
}
