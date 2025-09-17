import UserInfo from '../Domain/UserInfo';
import StatusRequest from './StatusRequest';
export default interface UserResult extends StatusRequest {
    user?: UserInfo;
}
