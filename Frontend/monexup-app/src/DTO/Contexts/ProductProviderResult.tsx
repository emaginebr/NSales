import ProductInfo from "../Domain/ProductInfo";
import ProviderResult from "./ProviderResult";

export default interface ProductProviderResult extends ProviderResult {
    product?: ProductInfo;
};