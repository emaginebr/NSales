import TemplatePartInfo from "../../DTO/Domain/TemplatePartInfo";
import TemplateVarInfo from "../../DTO/Domain/TemplateVarInfo";
import StatusRequest from "../../DTO/Services/StatusRequest";
import TemplatePageResult from "../../DTO/Services/TemplatePageResult";
import TemplateVarResult from "../../DTO/Services/TemplateVarResult";
import IHttpClient from "../../Infra/Interface/IHttpClient";

export default interface ITemplateService {
    init: (httpClient : IHttpClient) => void;
    getNetworkPage: (networkSlug: string, pageSlug: string, language: string) => Promise<TemplatePageResult>;
    getPageById: (pageId: number, language: string) => Promise<TemplatePageResult>;
    insertPart: (part: TemplatePartInfo, token: string) => Promise<StatusRequest>;
    updatePart: (part: TemplatePartInfo, token: string) => Promise<StatusRequest>;
    deletePart: (partId: number, token: string) => Promise<StatusRequest>;
    movePartUp: (partId: number, token: string) => Promise<StatusRequest>;
    movePartDown: (partId: number, token: string) => Promise<StatusRequest>;
    getVariable: (pageId: number, key: string, token: string) => Promise<TemplateVarResult>;
    saveVariable: (variable: TemplateVarInfo, token: string) => Promise<TemplateVarResult>;
}