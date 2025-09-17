import { OrderStatusEnum } from "../Enum/OrderStatusEnum";
import OrderItemInfo from "./OrderItemInfo";
import UserInfo from "./UserInfo";

export default interface OrderInfo {
    orderId: number;
    userId: number;
    sellerId?: number;
    createdAt: string;
    updatedAt: string;
    status: OrderStatusEnum;
    user: UserInfo;
    seller?: UserInfo;
    items: OrderItemInfo[];
}