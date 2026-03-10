import OrderInfo from "./OrderInfo";
import ProductInfo from "./ProductInfo";

export default interface OrderListPagedInfo {
    orders: OrderInfo[];
    pageNum: number;
    pageCount: number;
  }