import ProductInfo from "./ProductInfo";

export default interface ProductListPagedInfo {
    products: ProductInfo[];
    pageNum: number;
    pageCount: number;
  }