import StatusRequest from "./StatusRequest";

export default interface UserTokenResult extends StatusRequest {
  token? : string;
}