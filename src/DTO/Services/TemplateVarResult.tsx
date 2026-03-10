import TemplateVarInfo from "../Domain/TemplateVarInfo";
import StatusRequest from "./StatusRequest";

export default interface TemplateVarResult extends StatusRequest {
  variable: TemplateVarInfo;
}