import BusinessResult from "../../DTO/Business/BusinessResult";
import IImageService from "../../Services/Interfaces/IImageService";

export default interface IImageBusiness {
  init: (imageService: IImageService) => void;
  uploadImageUser: (file: Blob) => Promise<BusinessResult<string>>;
  uploadImageNetwork: (networkId: number, file: Blob) => Promise<BusinessResult<string>>;
  uploadImageProduct: (productId: number, file: Blob) => Promise<BusinessResult<string>>;
}