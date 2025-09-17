import ProductInfo from "../Domain/ProductInfo";
import StatusRequest from "./StatusRequest";

export default interface ProductListResult extends StatusRequest {
    products? : ProductInfo[];
  }