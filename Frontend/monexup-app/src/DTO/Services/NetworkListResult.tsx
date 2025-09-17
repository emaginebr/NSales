import NetworkInfo from "../Domain/NetworkInfo";
import StatusRequest from "./StatusRequest";

export default interface NetworkListResult extends StatusRequest {
  networks: NetworkInfo[];
}