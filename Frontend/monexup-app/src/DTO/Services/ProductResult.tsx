import ProductInfo from "../Domain/ProductInfo";
import StatusRequest from "./StatusRequest";

export default interface ProductResult extends StatusRequest {
  product?: ProductInfo;
}