import UserProfileInfo from "../Domain/UserProfileInfo";
import StatusRequest from "./StatusRequest";

export default interface UserProfileListResult extends StatusRequest {
  profiles: UserProfileInfo[];
}