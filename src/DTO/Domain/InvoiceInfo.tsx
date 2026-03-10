import { InvoiceStatusEnum } from "../Enum/InvoiceStatusEnum";
import InvoiceFeeInfo from "./InvoiceFeeInfo";
import OrderInfo from "./OrderInfo";
import UserInfo from "./UserInfo";

export default interface InvoiceInfo {
    invoiceId: number;
    orderId: number;
    userId: number;
    sellerId?: number;
    price: number;
    dueDate: string;
    paymentDate: string;
    status: InvoiceStatusEnum;
    order: OrderInfo;
    user: UserInfo;
    seller?: UserInfo;
    fees: InvoiceFeeInfo[];
}