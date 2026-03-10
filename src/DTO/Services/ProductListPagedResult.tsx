import ProductInfo from "../Domain/ProductInfo";
import StatusRequest from "./StatusRequest";

export default interface ProductListPagedResult extends StatusRequest {
    products: ProductInfo[];
    pageNum: number;
    pageCount: number;
  }