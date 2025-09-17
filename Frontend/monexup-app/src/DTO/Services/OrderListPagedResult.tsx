import OrderInfo from "../Domain/OrderInfo";
import StatusRequest from "./StatusRequest";

export default interface OrderListPagedResult extends StatusRequest {
  orders: OrderInfo[];
  pageNum: number;
  pageCount: number;
}