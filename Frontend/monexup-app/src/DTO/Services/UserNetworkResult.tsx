import UserNetworkInfo from "../Domain/UserNetworkInfo";
import StatusRequest from "./StatusRequest";

export default interface UserNetworkResult extends StatusRequest {
    userNetwork: UserNetworkInfo;
  }