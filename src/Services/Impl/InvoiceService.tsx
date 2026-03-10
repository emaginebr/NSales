import StatementSearchParam from "../../DTO/Domain/StatementSearchParam";
import InvoiceListPagedResult from "../../DTO/Services/InvoiceListPagedResult";
import InvoiceResult from "../../DTO/Services/InvoiceResult";
import NumberResult from "../../DTO/Services/NumberResult";
import StatementListPagedResult from "../../DTO/Services/StatementListPagedResult";
import StatusRequest from "../../DTO/Services/StatusRequest";
import IHttpClient from "../../Infra/Interface/IHttpClient"; 
import IInvoiceService from "../Interfaces/IInvoiceService";

let _httpClient : IHttpClient;

const InvoiceService : IInvoiceService = {
    init: function (htppClient: IHttpClient): void {
        _httpClient = htppClient;
    },
    search: async (networkId: number, userId: number, sellerId: number, pageNum: number, token: string) => {
        let ret: InvoiceListPagedResult;
        let request = await _httpClient.doPostAuth<InvoiceListPagedResult>("/Invoice/search", {
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
    searchStatement: async (param: StatementSearchParam, token: string) => {
        let ret: StatementListPagedResult;
        let request = await _httpClient.doPostAuth<StatementListPagedResult>("/Invoice/searchStatement", param, token);
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
    getBalance: async (token: string, networkId?: number) => {
        let ret: NumberResult;
        let url: string = "/api/Invoice/getBalance" + ((networkId) ? "?networkId=" + networkId : "");
        let request = await _httpClient.doGetAuth<NumberResult>(url, token);
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
    getAvailableBalance: async (token: string) => {
        let ret: NumberResult;
        let request = await _httpClient.doGetAuth<NumberResult>("/Invoice/getAvailableBalance", token);
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
    syncronize: async (token: string) => {
        let ret: StatusRequest;
        let request = await _httpClient.doGetAuth<StatusRequest>("/Invoice/syncronize", token);
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
    checkout: async (checkoutSessionId: string) => {
        let ret: InvoiceResult;
        let request = await _httpClient.doGet<InvoiceResult>("/Invoice/checkout/" + checkoutSessionId, {});
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

export default InvoiceService;