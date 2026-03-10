import ProviderResult from "./ProviderResult";

export default interface OrderProviderResult extends ProviderResult {
    clientSecret: string;
};