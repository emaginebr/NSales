import UserNetworkSearchInfo from "./UserNetworkSearchInfo";

export default interface UserListPagedInfo {
    users: UserNetworkSearchInfo[];
    pageNum: number;
    pageCount: number;
  }