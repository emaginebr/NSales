import OrderListPagedResult from "../../DTO/Services/OrderListPagedResult";
import OrderResult from "../../DTO/Services/OrderResult";
import SubscriptionResult from "../../DTO/Services/SubscriptionResult";
import IHttpClient from "../../Infra/Interface/IHttpClient";

export default interface IOrderService {
    init: (httpClient : IHttpClient) => void;
    createSubscription: (productSlug: string, token: string, networkSlug?: string, sellerSlug?: string) => Promise<SubscriptionResult>;
    createInvoice: (productSlug: string, token: string) => Promise<SubscriptionResult>;
    search: (networkId: number, userId: number, sellerId: number, pageNum: number, token: string) => Promise<OrderListPagedResult>;
    getById: (orderId: number, token: string) => Promise<OrderResult>;
}