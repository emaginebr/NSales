import InvoiceInfo from "../Domain/InvoiceInfo";
import StatusRequest from "./StatusRequest";

export default interface InvoiceResult extends StatusRequest {
  invoice: InvoiceInfo;
}