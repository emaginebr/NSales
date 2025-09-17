import NetworkInfo from "../Domain/NetworkInfo";
import StatusRequest from "./StatusRequest";

export default interface NetworkResult extends StatusRequest {
  network: NetworkInfo;
}