import InvoiceInfo from "../Domain/InvoiceInfo";
import StatusRequest from "./StatusRequest";

export default interface InvoiceListPagedResult extends StatusRequest {
  invoices: InvoiceInfo[];
  pageNum: number;
  pageCount: number;
}