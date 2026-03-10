import UserInfo from '../Domain/UserInfo';
import StatusRequest from './StatusRequest';
export default interface UserListResult extends StatusRequest {
    users?: UserInfo[];
}
