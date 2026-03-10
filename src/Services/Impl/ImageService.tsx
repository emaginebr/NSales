import StringResult from "../../DTO/Services/StringResult";
import IHttpClient from "../../Infra/Interface/IHttpClient"; 
import IImageService from "../Interfaces/IImageService";

let _httpClient : IHttpClient;

const ImageService : IImageService = {
    init: function (htppClient: IHttpClient): void {
        _httpClient = htppClient;
    },
    uploadImageUser: async (file: Blob, token: string) => {
        let ret: StringResult;

        const formData = new FormData();
        formData.append('file', file, 'cropped.jpg');

        //formData.append("networkId", "0");
        let request = await _httpClient.doPostFormDataAuth<StringResult>("/Image/uploadImageUser", formData, token);
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
    uploadImageNetwork: async (networkId: number, file: Blob, token: string) => {
        let ret: StringResult;

        const formData = new FormData();
        formData.append('file', file, 'cropped.jpg');
        formData.append('networkId', networkId.toString());

        let request = await _httpClient.doPostFormDataAuth<StringResult>("/Image/uploadImageNetwork", formData, token);
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
    uploadImageProduct: async (productId: number, file: Blob, token: string) => {
        let ret: StringResult;

        const formData = new FormData();
        formData.append('file', file, 'cropped.jpg');
        formData.append('productId', productId.toString());

        let request = await _httpClient.doPostFormDataAuth<StringResult>("/Image/uploadImageProduct", formData, token);
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
}

export default ImageService;