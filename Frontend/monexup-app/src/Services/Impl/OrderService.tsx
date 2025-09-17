import OrderListPagedResult from "../../DTO/Services/OrderListPagedResult";
import OrderResult from "../../DTO/Services/OrderResult";
import SubscriptionResult from "../../DTO/Services/SubscriptionResult";
import IHttpClient from "../../Infra/Interface/IHttpClient"; 
import IOrderService from "../Interfaces/IOrderService";

let _httpClient : IHttpClient;

const OrderService : IOrderService = {
    init: function (htppClient: IHttpClient): void {
        _httpClient = htppClient;
    },
    createSubscription: async (productSlug: string, token: string, networkSlug?: string, sellerSlug?: string) => {
        let ret: SubscriptionResult;
        let url: string = "/Order/createSubscription/" + productSlug;
        if (networkSlug) {
            url += "?networkSlug=" + networkSlug;
        }
        if (sellerSlug) {
            url += "?sellerSlug=" + sellerSlug;
        }
        let request = await _httpClient.doGetAuth<SubscriptionResult>(url, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    createInvoice: async (productSlug: string, token: string) => {
        let ret: SubscriptionResult;
        let request = await _httpClient.doGetAuth<SubscriptionResult>("/Order/createInvoice/" + productSlug, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    search: async (networkId: number, userId: number, sellerId: number, pageNum: number, token: string) => {
        let ret: OrderListPagedResult;
        let request = await _httpClient.doPostAuth<OrderListPagedResult>("/Order/search", {
            networkId: networkId,
            userId: userId,
            sellerId: sellerId,
            pageNum: pageNum
        }, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    getById: async (orderId: number, token: string) => {
        let ret: OrderResult;
        let request = await _httpClient.doGetAuth<OrderListPagedResult>("/Order/getById/" + orderId, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    }
}

export default OrderService;