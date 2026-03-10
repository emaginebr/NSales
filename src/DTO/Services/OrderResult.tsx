import OrderInfo from "../Domain/OrderInfo";
import StatusRequest from "./StatusRequest";

export default interface OrderResult extends StatusRequest {
    order? : OrderInfo;
  }