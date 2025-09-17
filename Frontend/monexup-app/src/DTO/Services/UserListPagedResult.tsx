import UserNetworkSearchInfo from "../Domain/UserNetworkSearchInfo";
import StatusRequest from "./StatusRequest";

export default interface UserListPagedResult extends StatusRequest {
  users: UserNetworkSearchInfo[];
  pageNum: number;
  pageCount: number;
}