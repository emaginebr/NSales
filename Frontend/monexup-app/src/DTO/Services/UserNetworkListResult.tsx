import UserNetworkInfo from "../Domain/UserNetworkInfo";
import StatusRequest from "./StatusRequest";

export default interface UserNetworkListResult extends StatusRequest {
  userNetworks: UserNetworkInfo[];
}