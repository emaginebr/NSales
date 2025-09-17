import NetworkInfo from "../Domain/NetworkInfo";
import ProviderResult from "./ProviderResult";

export default interface NetworkProviderResult extends ProviderResult {
    network?: NetworkInfo;
};