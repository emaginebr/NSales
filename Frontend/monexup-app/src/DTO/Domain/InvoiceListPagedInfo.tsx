import InvoiceInfo from "./InvoiceInfo";

export default interface InvoiceListPagedInfo {
    invoices: InvoiceInfo[];
    pageNum: number;
    pageCount: number;
  }