import BusinessResult from "../../DTO/Business/BusinessResult";
import TemplatePageInfo from "../../DTO/Domain/TemplatePageInfo";
import TemplatePartInfo from "../../DTO/Domain/TemplatePartInfo";
import TemplateVarInfo from "../../DTO/Domain/TemplateVarInfo";
import { LanguageEnum } from "../../DTO/Enum/LanguageEnum";
import ITemplateService from "../../Services/Interfaces/ITemplateService";

export default interface ITemplateBusiness {
    init: (templateService: ITemplateService) => void;
    getNetworkPage: (networkSlug: string, pageSlug: string, lang: LanguageEnum) => Promise<BusinessResult<TemplatePageInfo>>;
    getPageById: (pageId: number, lang: LanguageEnum) => Promise<BusinessResult<TemplatePageInfo>>;
    insertPart: (part: TemplatePartInfo) => Promise<BusinessResult<Boolean>>;
    updatePart: (part: TemplatePartInfo) => Promise<BusinessResult<Boolean>>;
    deletePart: (partId: number) => Promise<BusinessResult<Boolean>>;
    movePartUp: (partId: number) => Promise<BusinessResult<Boolean>>;
    movePartDown: (partId: number) => Promise<BusinessResult<Boolean>>;
    getVariable: (pageId: number, key: string) => Promise<BusinessResult<TemplateVarInfo>>;
    saveVariable: (variable: TemplateVarInfo) => Promise<BusinessResult<TemplateVarInfo>>;
}