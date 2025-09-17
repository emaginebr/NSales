import { langToStr } from "../../Components/Functions";
import BusinessResult from "../../DTO/Business/BusinessResult";
import AuthSession from "../../DTO/Domain/AuthSession";
import TemplatePageInfo from "../../DTO/Domain/TemplatePageInfo";
import TemplatePartInfo from "../../DTO/Domain/TemplatePartInfo";
import TemplateVarInfo from "../../DTO/Domain/TemplateVarInfo";
import UserProfileInfo from "../../DTO/Domain/UserProfileInfo";
import { LanguageEnum } from "../../DTO/Enum/LanguageEnum";
import IProfileService from "../../Services/Interfaces/IProfileService";
import ITemplateService from "../../Services/Interfaces/ITemplateService";
import AuthFactory from "../Factory/AuthFactory";
import IProfileBusiness from "../Interfaces/IProfileBusiness";
import ITemplateBusiness from "../Interfaces/ITemplateBusiness";

let _templateService: ITemplateService;

const TemplateBusiness: ITemplateBusiness = {
  init: function (templateService: ITemplateService): void {
    _templateService = templateService;
  },
  getNetworkPage: async (networkSlug: string, pageSlug: string, lang: LanguageEnum) => {
    //try {
        let ret: BusinessResult<TemplatePageInfo>;
        let retServ = await _templateService.getNetworkPage(networkSlug, pageSlug, langToStr(lang));
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.page,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      /*  
      } catch {
        throw new Error("Failed to insert");
      }
      */
  },
  getPageById: async (pageId: number, lang: LanguageEnum) => {
    //try {
        let ret: BusinessResult<TemplatePageInfo>;
        let retServ = await _templateService.getPageById(pageId, langToStr(lang));
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.page,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      /*  
      } catch {
        throw new Error("Failed to insert");
      }
      */
  },
  insertPart: async (part: TemplatePartInfo) => {
    //try {
        let ret: BusinessResult<Boolean>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _templateService.insertPart(part, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.sucesso,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      /*  
      } catch {
        throw new Error("Failed to insert");
      }
      */
  },
  updatePart: async (part: TemplatePartInfo) => {
    //try {
        let ret: BusinessResult<Boolean>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _templateService.updatePart(part, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.sucesso,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      /*  
      } catch {
        throw new Error("Failed to insert");
      }
      */
  },
  deletePart: async (partId: number) => {
    //try {
        let ret: BusinessResult<Boolean>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _templateService.deletePart(partId, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.sucesso,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      /*  
      } catch {
        throw new Error("Failed to insert");
      }
      */
  },
  movePartUp: async (partId: number) => {
    //try {
        let ret: BusinessResult<Boolean>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _templateService.movePartUp(partId, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.sucesso,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      /*  
      } catch {
        throw new Error("Failed to insert");
      }
      */
  },
  movePartDown: async (partId: number) => {
    //try {
        let ret: BusinessResult<Boolean>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _templateService.movePartDown(partId, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.sucesso,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      /*  
      } catch {
        throw new Error("Failed to insert");
      }
      */
  },
  getVariable: async (partId: number, key: string) => {
    //try {
        let ret: BusinessResult<TemplateVarInfo>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _templateService.getVariable(partId, key, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.variable,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      /*  
      } catch {
        throw new Error("Failed to insert");
      }
      */
  },
  saveVariable: async (variable: TemplateVarInfo) => {
    //try {
        let ret: BusinessResult<TemplateVarInfo>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _templateService.saveVariable(variable, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.variable,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      /*  
      } catch {
        throw new Error("Failed to insert");
      }
      */
  }
}

export default TemplateBusiness;