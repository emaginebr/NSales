import StatementSearchParam from "../../DTO/Domain/StatementSearchParam";
import InvoiceListPagedResult from "../../DTO/Services/InvoiceListPagedResult";
import InvoiceResult from "../../DTO/Services/InvoiceResult";
import NumberResult from "../../DTO/Services/NumberResult";
import StatementListPagedResult from "../../DTO/Services/StatementListPagedResult";
import StatusRequest from "../../DTO/Services/StatusRequest";
import IHttpClient from "../../Infra/Interface/IHttpClient";

export default interface IInvoiceService {
    init: (httpClient : IHttpClient) => void;
    search: (networkId: number, userId: number, sellerId: number, pageNum: number, token: string) => Promise<InvoiceListPagedResult>;
    searchStatement: (param: StatementSearchParam, token: string) => Promise<StatementListPagedResult>;
    getBalance: (token: string, networkId?: number) => Promise<NumberResult>;
    getAvailableBalance: (token: string) => Promise<NumberResult>;
    syncronize: (token: string) => Promise<StatusRequest>;
    checkout: (checkoutSessionId: string) => Promise<InvoiceResult>;
}