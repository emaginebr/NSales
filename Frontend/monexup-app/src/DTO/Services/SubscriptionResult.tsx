import OrderInfo from "../Domain/OrderInfo";
import StatusRequest from "./StatusRequest";

export default interface SubscriptionResult extends StatusRequest {
    order : OrderInfo;
    clientSecret: string;
}