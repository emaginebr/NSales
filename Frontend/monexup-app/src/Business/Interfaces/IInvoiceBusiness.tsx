import BusinessResult from "../../DTO/Business/BusinessResult";
import InvoiceInfo from "../../DTO/Domain/InvoiceInfo";
import InvoiceListPagedInfo from "../../DTO/Domain/InvoiceListPagedInfo";
import StatementListPagedInfo from "../../DTO/Domain/StatementListPagedInfo";
import StatementSearchParam from "../../DTO/Domain/StatementSearchParam";
import IInvoiceService from "../../Services/Interfaces/IInvoiceService";

export default interface IInvoiceBusiness {
  init: (invoiceService: IInvoiceService) => void;
  search: (networkId: number, userId: number, sellerId: number, pageNum: number) => Promise<BusinessResult<InvoiceListPagedInfo>>;
  searchStatement: (param: StatementSearchParam) => Promise<BusinessResult<StatementListPagedInfo>>;
  getBalance: (networkId?: number) => Promise<BusinessResult<number>>;
  getAvailableBalance: () => Promise<BusinessResult<number>>;
  syncronize: () => Promise<BusinessResult<boolean>>;
  checkout: (checkoutSessionId: string) => Promise<BusinessResult<InvoiceInfo>>;
}