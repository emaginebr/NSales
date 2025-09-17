import TemplatePartInfo from "../../DTO/Domain/TemplatePartInfo";
import TemplateVarInfo from "../../DTO/Domain/TemplateVarInfo";
import StatusRequest from "../../DTO/Services/StatusRequest";
import TemplatePageResult from "../../DTO/Services/TemplatePageResult";
import TemplateVarResult from "../../DTO/Services/TemplateVarResult";
import IHttpClient from "../../Infra/Interface/IHttpClient"; 
import ITemplateService from "../Interfaces/ITemplateService";

let _httpClient : IHttpClient;

const TemplateService : ITemplateService = {
    init: function (htppClient: IHttpClient): void {
        _httpClient = htppClient;
    },
    getNetworkPage: async (networkSlug: string, pageSlug: string, language: string) => {
        let ret: TemplatePageResult;
        let url: string = "/Template/getNetworkPage/" + networkSlug + "/" + pageSlug + "/" + language;
        let request = await _httpClient.doGet<TemplatePageResult>(url, {});
        if (request.success) {
            console.log("Template Page:", JSON.stringify(request.data));
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
    getPageById: async (pageId: number, language: string) => {
        let ret: TemplatePageResult;
        let url: string = "/Template/getPageById/" + pageId + "/" + language;
        let request = await _httpClient.doGet<TemplatePageResult>(url, {});
        if (request.success) {
            console.log("Template Page:", JSON.stringify(request.data));
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
    insertPart: async (part: TemplatePartInfo, token: string) => {
        let ret: StatusRequest;
        let request = await _httpClient.doPostAuth<StatusRequest>("/Template/insertPart", part, token);
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
    updatePart: async (part: TemplatePartInfo, token: string) => {
        let ret: StatusRequest;
        let request = await _httpClient.doPostAuth<StatusRequest>("/Template/updatePart", part, token);
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
    deletePart: async (partId: number, token: string) => {
        let ret: StatusRequest;
        let request = await _httpClient.doGetAuth<StatusRequest>("/Template/deletePart/" + partId, token);
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
    movePartUp: async (partId: number, token: string) => {
        let ret: StatusRequest;
        let request = await _httpClient.doGetAuth<StatusRequest>("/Template/movePartUp/" + partId, token);
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
    movePartDown: async (partId: number, token: string) => {
        let ret: StatusRequest;
        let request = await _httpClient.doGetAuth<StatusRequest>("/Template/movePartDown/" + partId, token);
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
    getVariable: async (pageId: number, key: string, token: string) => {
        let ret: TemplateVarResult;
        let url: string = "/Template/getVariable/" + pageId + "/" + key;
        let request = await _httpClient.doGetAuth<TemplateVarResult>(url, token);
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
    saveVariable: async (variable: TemplateVarInfo, token: string) => {
        let ret: TemplateVarResult;
        let url: string = "/Template/saveVariable";
        let request = await _httpClient.doPostAuth<TemplateVarResult>(url, variable, token);
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

export default TemplateService;