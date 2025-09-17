import BusinessResult from "../../DTO/Business/BusinessResult";
import AuthSession from "../../DTO/Domain/AuthSession";
import IImageService from "../../Services/Interfaces/IImageService";
import AuthFactory from "../Factory/AuthFactory";
import IImageBusiness from "../Interfaces/IImageBusiness";

let _imageService: IImageService;

const ImageBusiness: IImageBusiness = {
  init: function (imageService: IImageService): void {
    _imageService = imageService;
  },
  uploadImageUser: async (file: Blob) => {
    try {
      let ret: BusinessResult<string>;
      let session: AuthSession = AuthFactory.AuthBusiness.getSession();
      if (!session) {
        return {
          ...ret,
          sucesso: false,
          mensagem: "Not logged"
        };
      }
      let retServ = await _imageService.uploadImageUser(file, session.token);
      if (retServ.sucesso) {
        return {
          ...ret,
          dataResult: retServ.value,
          sucesso: true
        };
      } else {
        return {
          ...ret,
          sucesso: false,
          mensagem: retServ.mensagem
        };
      }
    } catch {
      throw new Error("Failed to get user by email");
    }
  },
  uploadImageNetwork: async (networkId: number, file: Blob) => {
    try {
      let ret: BusinessResult<string>;
      let session: AuthSession = AuthFactory.AuthBusiness.getSession();
      if (!session) {
        return {
          ...ret,
          sucesso: false,
          mensagem: "Not logged"
        };
      }
      let retServ = await _imageService.uploadImageNetwork(networkId, file, session.token);
      if (retServ.sucesso) {
        return {
          ...ret,
          dataResult: retServ.value,
          sucesso: true
        };
      } else {
        return {
          ...ret,
          sucesso: false,
          mensagem: retServ.mensagem
        };
      }
    } catch {
      throw new Error("Failed to get user by email");
    }
  },
  uploadImageProduct: async (productId: number, file: Blob) => {
    try {
      let ret: BusinessResult<string>;
      let session: AuthSession = AuthFactory.AuthBusiness.getSession();
      if (!session) {
        return {
          ...ret,
          sucesso: false,
          mensagem: "Not logged"
        };
      }
      let retServ = await _imageService.uploadImageProduct(productId, file, session.token);
      if (retServ.sucesso) {
        return {
          ...ret,
          dataResult: retServ.value,
          sucesso: true
        };
      } else {
        return {
          ...ret,
          sucesso: false,
          mensagem: retServ.mensagem
        };
      }
    } catch {
      throw new Error("Failed to get user by email");
    }
  },
}

export default ImageBusiness;