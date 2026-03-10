import UserProfileInfo from "../Domain/UserProfileInfo";
import StatusRequest from "./StatusRequest";

export default interface UserProfileResult extends StatusRequest {
  profile: UserProfileInfo;
}