import UserInfo from '../Domain/UserInfo';
import StatusRequest from './StatusRequest';
export default interface UserListPagedResult extends StatusRequest {
    users: UserInfo[];
    pageNum: number;
    pageCount: number;
}
