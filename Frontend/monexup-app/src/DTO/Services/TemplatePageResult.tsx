import TemplatePageInfo from "../Domain/TemplatePageInfo";
import StatusRequest from "./StatusRequest";

export default interface TemplatePageResult extends StatusRequest {
  page: TemplatePageInfo;
}